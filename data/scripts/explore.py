import pandas as pd

df = pd.read_csv("data/raw/dinesafe.csv", dtype={"phone": str, "oldEstId": str})
df["inspectionDate"] = pd.to_datetime(df["inspectionDate"])

print("Inspection status values:")
print(df["inspectionStatus"].value_counts(dropna=False))

print("\nSeverity values:")
print(df["severity"].value_counts(dropna=False))

print("\nDate range:", df["inspectionDate"].min(), "to", df["inspectionDate"].max())

print("\nUnique establishments:", df["estId"].nunique())
print("Unique inspections (estId + date):", df.groupby(["estId", "inspectionDate"]).ngroups)

print("\nMissing values per column:")
print(df.isna().sum())