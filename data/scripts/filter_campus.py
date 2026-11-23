import re
import numpy as np
import pandas as pd

CAMPUS_LAT, CAMPUS_LON = 43.7846, -79.2270

df = pd.read_csv("data/raw/dinesafe.csv", dtype={"phone": str, "oldEstId": str})
df["inspectionDate"] = pd.to_datetime(df["inspectionDate"])
df = df[df["inspectionStatus"] != "Temporarily Not Operating"]

# --- Clean text so duplicates match ---
def norm_name(s):
    s = s.upper().replace("&", " AND ")
    s = re.sub(r"[^A-Z0-9 ]", " ", s)          # remove punctuation
    return re.sub(r"\s+", " ", s).strip()

df["address"] = df["address"].str.replace(" None", "", regex=False).str.strip()
df["postalCode"] = df["address"].str[-7:]       # e.g. "M1G 3T8"
df["nameKey"] = df["estName"].apply(norm_name) + "|" + df["postalCode"]

# --- Exclude places that aren't restaurants ---
EXCLUDE = [r"CHILD CARE", r"DAY ?CARE", r"MONTESSORI", r"\bP S\b", r"COLLEGIATE",
           r"\bSNP\b", r"SNACK PROGRAM", r"MORNING MEAL", r"FOOD BANK", r"SHELTER",
           r"CHURCH", r"TEMPLE", r"RESIDENCE", r"EDUCATIONAL", r"DEVELOPMENTAL",
           r"BOYS AND GIRLS", r"SCHOOL", r"ACADEMY", r"\bMART\b", r"SUPER ?MARKET",
           r"GROCERY", r"PETRO CANADA",r"STUDENT NUTRITION", r"RETIREMENT", r"RESIDENTIAL", r"LEARNING CENTRE",
           r"CONVENIENCE", r"WHOLESALE"]
pattern = "|".join(EXCLUDE)
ALLOW = r"RESTAURANT|CHICKEN|SUSHI|SWEET TREATS|GOLD COAST"
is_excluded = df["nameKey"].str.contains(pattern) & ~df["nameKey"].str.contains(ALLOW)
excluded = df[is_excluded]
df = df[~is_excluded]

# Manual review: private/staff kitchens and food retail (documented in the report)
MANUAL_EXCLUDE = ["TEVA CAFETERIA", "SKF", "ARAMARK CANADA", "CENTENNIAL COLLEGE CULINARY LABS",
                  "5N2", "SCARBOROUGH PROGRESS CENTRE", "FOOD BASICS",
                  "THE MEATHEAD STORE", "BEAVER FISHERY"]
df = df[~df["estName"].str.upper().isin(MANUAL_EXCLUDE)]

# --- Merge duplicates: keep the estId with the most recent inspection ---
latest = df.groupby(["nameKey", "estId"])["inspectionDate"].max().reset_index()
keep = latest.sort_values("inspectionDate").groupby("nameKey").tail(1)
id_map = dict(zip(latest["estId"], latest["nameKey"].map(dict(zip(keep["nameKey"], keep["estId"])))))
df["estId"] = df["estId"].map(id_map)

# --- One row per establishment + distance ---
places = df.groupby("estId").agg(
    estName=("estName", "first"), address=("address", "first"),
    latitude=("latitude", "first"), longitude=("longitude", "first"),
).reset_index()
lat1, lon1 = np.radians(CAMPUS_LAT), np.radians(CAMPUS_LON)
lat2, lon2 = np.radians(places["latitude"]), np.radians(places["longitude"])
a = np.sin((lat2-lat1)/2)**2 + np.cos(lat1)*np.cos(lat2)*np.sin((lon2-lon1)/2)**2
places["distanceKm"] = 6371 * 2 * np.arcsin(np.sqrt(a))

nearest = places.sort_values("distanceKm").head(100)
subset = df[df["estId"].isin(nearest["estId"])]
inspections = subset.groupby(["estId", "inspectionDate"])["inspectionStatus"].first()

print("Excluded within 2 km of campus (check these):")
exc_places = excluded.drop_duplicates("estId")
d = 6371 * 2 * np.arcsin(np.sqrt(
    np.sin((np.radians(exc_places["latitude"]) - lat1) / 2) ** 2
    + np.cos(lat1) * np.cos(np.radians(exc_places["latitude"]))
    * np.sin((np.radians(exc_places["longitude"]) - lon1) / 2) ** 2))
print(sorted(exc_places[d < 2]["estName"].unique()))

# Nearest establishments with a Closed inspection, in case we need to add some
closed_ids = df[df["inspectionStatus"] == "Closed"]["estId"].unique()
closed = places[places["estId"].isin(closed_ids)].sort_values("distanceKm").head(5)
print("\nNearest 5 with a Closed inspection:")
print(closed[["estName", "address", "distanceKm"]].to_string(index=False))

# final 100
print("\nFinal 100:")
print(nearest[["estName", "address", "distanceKm"]].to_string(index=False))

# ===================== BUILD SEED TABLES =====================

# --- Establishments: one row per place, with our own integer ID ---
est = nearest.sort_values("distanceKm").reset_index(drop=True)
est["EstablishmentId"] = est.index + 1

postal_re = r"([A-Za-z]\d[A-Za-z] ?\d[A-Za-z]\d)$"
est["PostalCode"] = est["address"].str.extract(postal_re, expand=False).str.upper()
est["Street"] = est["address"].str.replace(postal_re, "", regex=True).str.strip(" ,")

phones = subset.groupby("estId")["phone"].first()
est["Phone"] = est["estId"].map(phones).str.replace(r"\D", "", regex=True)

establishments = est.rename(columns={"estId": "DineSafeId", "estName": "Name",
                                     "latitude": "Latitude", "longitude": "Longitude"})[
    ["EstablishmentId", "DineSafeId", "Name", "Street", "PostalCode", "Phone",
     "Latitude", "Longitude"]]

# --- Inspections: one row per establishment + date ---
rows = subset.drop_duplicates(["estId", "inspectionDate", "typeDesc"]).copy()
rows["sev"] = rows["severity"].str[0]              # "C", "S", "M" or NaN
rows["rank"] = rows["sev"].map({"C": 0, "S": 1, "M": 2})

def clean_infraction(text):
    if pd.isna(text):
        return "No infractions found"
    text = re.sub(r"\s*\S?\s*\bSEC\b\.?.*$", "", text, flags=re.IGNORECASE)     # drop the "- SEC. 22" legal reference
    return text.capitalize()

g = rows.sort_values("rank").groupby(["estId", "inspectionDate"])
inspections = g.agg(
    Status=("inspectionStatus", "first"),
    InfractionCount=("typeDesc", "count"),
    CrucialCount=("sev", lambda s: (s == "C").sum()),
    SignificantCount=("sev", lambda s: (s == "S").sum()),
    MinorCount=("sev", lambda s: (s == "M").sum()),
    InfractionSummary=("typeDesc", "first"),
    Observation=("observation", "first"),
).reset_index()

inspections["InfractionSummary"] = inspections["InfractionSummary"].apply(clean_infraction)
inspections["EstablishmentId"] = inspections["estId"].map(dict(zip(est["estId"], est["EstablishmentId"])))
inspections = inspections.sort_values(["EstablishmentId", "inspectionDate"]).reset_index(drop=True)
inspections["InspectionId"] = inspections.index + 1
inspections["InspectionDate"] = inspections["inspectionDate"].dt.strftime("%Y-%m-%d")
inspections = inspections[["InspectionId", "EstablishmentId", "InspectionDate", "Status",
                           "InfractionCount", "CrucialCount", "SignificantCount", "MinorCount",
                           "InfractionSummary", "Observation"]]

# --- Export ---
establishments.to_csv("data/seed/establishments.csv", index=False)
inspections.to_csv("data/seed/inspections.csv", index=False)

print("\nEstablishments:", len(establishments), "| missing postal codes:", establishments["PostalCode"].isna().sum())
print("Inspections:", len(inspections))
print(inspections["Status"].value_counts())
print("\nSample inspections:")
print(inspections.head(5).to_string(index=False))