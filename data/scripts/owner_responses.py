import random
import pandas as pd

random.seed(306)  # same output every run (COMP306!)

est = pd.read_csv("data/seed/establishments.csv")
ins = pd.read_csv("data/seed/inspections.csv", parse_dates=["InspectionDate"])
ins = ins.merge(est[["EstablishmentId", "Name"]], on="EstablishmentId")

# Priority: 0 = Closed, 1 = Conditional Pass, 2 = Pass with infractions, 3 = clean Pass
def priority(row):
    if row["Status"] == "Closed":
        return 0
    if row["Status"] == "Conditional Pass":
        return 1
    return 2 if row["InfractionCount"] > 0 else 3

ins["priority"] = ins.apply(priority, axis=1)
chosen = ins.sort_values(["priority", "InspectionDate"], ascending=[True, False]).head(100)

TEMPLATES = {
    0: ["We sincerely apologize. After the inspection on {date} ({issue}), we closed to fix the problem and passed our re-inspection.",
        "Our premises were closed following the {date} inspection. We have corrected the issue ({issue}) and retrained all staff."],
    1: ["Thank you to Toronto Public Health. The issue found on {date} ({issue}) was corrected the same week.",
        "We take food safety seriously. Since the {date} inspection we have fixed the following: {issue}.",
        "After our conditional pass on {date}, we addressed the issue ({issue}) and updated our daily checklist."],
    2: ["We passed our {date} inspection and have already fixed the minor issue noted: {issue}.",
        "Thanks to the inspector for the feedback on {date}. We corrected this right away: {issue}."],
    3: ["Proud of our team for a clean inspection on {date}. Thank you for dining with us!",
        "No infractions found on {date}. Food safety is part of everything we do."],
}

rows = []
for i, (_, r) in enumerate(chosen.iterrows(), start=1):
    issue = r["InfractionSummary"].lower()
    message = random.choice(TEMPLATES[r["priority"]]).format(
        date=r["InspectionDate"].strftime("%B %d, %Y"), issue=issue)
    created = r["InspectionDate"] + pd.Timedelta(days=random.randint(2, 14))
    rows.append({
        "OwnerResponseId": i,
        "InspectionId": r["InspectionId"],
        "OwnerName": f"Management, {r['Name']}",
        "Message": message,
        "CreatedAt": created.strftime("%Y-%m-%d"),
    })

responses = pd.DataFrame(rows)
responses.to_csv("data/seed/owner_responses.csv", index=False)

print("Owner responses:", len(responses))
print(chosen["priority"].value_counts().sort_index())
print(responses.head(5).to_string(index=False))