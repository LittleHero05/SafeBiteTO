# SafeBite TO: project outline

COMP306, Group 3: Isabella Porras, Isabella Cao, Fatimah Rana.
This file keeps the team on the same page. The full proposal is in Google Docs: [Project-Proposal-Group3-COMP306](https://docs.google.com/document/d/1s4HHWRE3CfGFQgjOL4wJ8Sp3y1fn0B9_FhyIa1DX4mk/edit).

## What we're building

SafeBite TO is a RESTful API that turns Toronto Public Health's [DineSafe](https://open.toronto.ca/dataset/dinesafe/) inspection data into clean, ready-to-display food safety badges (Pass, Conditional Pass, Closed). Third-party apps call our API instead of each processing the City's raw file themselves.

- **Problem:** DineSafe is public, but it's a large daily file with one row per infraction and codes like S, M and C. Delivery and review apps don't show it.
- **What we add:** one badge per restaurant, plain-language infractions, a 12-month trend, and owner responses (data the City doesn't have).
- **Consumers (Apigee developers):** food delivery apps, review sites and food blogs, a restaurant owner portal, neighbourhood and real estate apps.

## Requirements we're graded on (marking scheme)

- [ ] Data in the cloud (RDS): each table has at least 20 rows of **real** data. *The professor may have said 100 in class; confirm. We plan for 100.*
- [ ] Repository pattern
- [ ] At least 3 DTO classes per controller, with an AutoMapper mapping profile
- [ ] At least 3 resource controllers, each with GetAll, GetById, POST, PUT, PATCH, DELETE
- [ ] API containerized, image pushed to ECR
- [ ] Deployed to ECS on Fargate: task definition registered, standalone task run, API reachable
- [ ] Apigee: proxy (target endpoint shown), product, 3 developers, Verify API Key (screenshots with and without a key), developer portal
- [ ] Client app using HttpClient with the API key, invoking all 6 methods on every controller
- [ ] Report: architecture, data model (ER diagram and sample data), lessons learnt, references
- [ ] Live demo (no recordings). Code and report due Sunday of Week 11, 11:59 PM; one member submits.

## Architecture

| Part | Choice |
| --- | --- |
| API | ASP.NET Core Web API (.NET 10), EF Core 10 code first, repository pattern, DTOs + AutoMapper, JSON Patch |
| API docs | Named OpenAPI document registered in Program.cs, Swagger UI, tested with Visual Studio Endpoints Explorer |
| Database | Amazon RDS for SQL Server (local development: SQL Server in Docker on the Mac) |
| API hosting | Docker image (linux/arm64) in Amazon ECR, standalone task on Amazon ECS with AWS Fargate |
| API management | Google Apigee: proxy, product, developers, app, Verify API Key, portal |
| Client | ASP.NET Core MVC (.NET 10), Razor views, Bootstrap 5, jQuery validation, on Elastic Beanstalk |
| Other AWS | Parameter Store (API URL and API key for the client), S3 (raw DineSafe snapshot), DynamoDB (optional: saved restaurants) |

Request flow: Client (MVC) → Apigee (checks the API key) → ECS Fargate task (our API) → RDS SQL Server.

## Data model (seed data ready in `data/seed/`)

Built from DineSafe by `data/scripts/filter_campus.py` and `data/scripts/owner_responses.py`: the 100 food establishments closest to Centennial's Progress Campus (within about 1.8 km).

| Table | Rows | Columns | Source |
| --- | --- | --- | --- |
| Establishments | 100 | EstablishmentId (PK), DineSafeId, Name, Street, PostalCode (nullable), Phone (nullable), Latitude, Longitude | DineSafe |
| Inspections | 447 | InspectionId (PK), EstablishmentId (FK), InspectionDate, Status, InfractionCount, CrucialCount, SignificantCount, MinorCount, InfractionSummary, Observation | DineSafe, grouped from infraction rows |
| OwnerResponses | 100 | OwnerResponseId (PK), InspectionId (FK), OwnerName, Message, CreatedAt | Written by us from real inspection details |

Inspection statuses: 404 Pass, 41 Conditional Pass, 2 Closed.

**Cleaning decisions (for the report):**
- DineSafe has one row per infraction and no inspection ID, so an inspection = establishment + date. We created our own integer IDs and kept the City's ID as `DineSafeId`.
- Excluded places that aren't restaurants (schools, child care, food banks, shelters, places of worship, residences, grocery and convenience stores) with keyword rules plus an allow list, and a short manual list of private kitchens and food retail.
- Merged duplicate establishments recorded under two IDs (same name and postal code).
- Infraction summary = the most serious infraction, with the legal section reference removed.
- Some infractions have no severity in the source, so the three severity counts can add up to less than `InfractionCount`.
- Owner responses are generated from templates filled with each inspection's real date and infraction (fixed random seed 306, so everyone gets the same file).

## API endpoints

Each controller has the same six operations:

| Operation | Example |
| --- | --- |
| GetAll | `GET /api/establishments` |
| GetById | `GET /api/establishments/{id}` |
| Create | `POST /api/establishments` |
| Replace | `PUT /api/establishments/{id}` |
| Partial update | `PATCH /api/establishments/{id}` (JSON Patch document) |
| Delete | `DELETE /api/establishments/{id}` |

Same pattern for `/api/inspections` and `/api/owner-responses`. Extra read endpoint: `GET /api/establishments/{id}/badge` returns the flattened badge.

**DTOs per controller:** `ReadDto`, `CreateDto`, `UpdateDto` (PATCH applies a `JsonPatchDocument<UpdateDto>`), plus `BadgeDto` for the badge view.

**What POST/PUT/PATCH/DELETE mean for us:** they change our own copy in RDS, never the City's file. The marking scheme allows these to be internal or admin actions as long as they work. Owner responses is the natural place for client forms; establishment and inspection forms are an admin tool. We load a DineSafe snapshot; live syncing is a nice-to-have.

## Professor's notes from class

- ASP.NET Core Web API, **code first** (EF Core migrations)
- Use **Endpoints Explorer** in Visual Studio
- Register a **named OpenAPI document** in Program.cs (shown in Swagger)
- **Containerize** the API
- Use **DTOs**: hide sensitive properties, reduce payload size, flatten complex object graphs
- Map models and DTOs with **AutoMapper** (NuGet package)
- **JSON Patch** with **Newtonsoft.Json**: `Microsoft.AspNetCore.JsonPatch`, `Microsoft.AspNetCore.Mvc.NewtonsoftJson`
- Report: include appropriate **references**

### AWS and ECS notes (from class)

- IAM user (the one from the labs) with `AmazonEC2ContainerRegistryFullAccess` and `PowerUserAccess`
- Region: **us-east-1** for everything
- Task definition: launch type **Fargate**, OS/architecture **Linux/ARM64** (images built on our Macs are ARM)
- Go to Task definitions → [task] → [revision] → **Run task**. Do **not** create a service.
- Networking: default VPC. If there isn't one, create a VPC with at least 2 public subnets (route table pointing to an internet gateway) and a security group that allows the API port inbound.
- **Public IP turned on**
- Copy the task definition JSON and save it as `docs/task-definition.json`. It's part of the submission.

## How to run the API locally

1. Clone the repo and open `SafeBiteTO.slnx` in Visual Studio.
2. Start SQL Server (for example the Docker container from Lab 3).
3. Right-click **SafeBite.Api → Manage User Secrets** and add your own connection string (never commit it):
   ```json
   { "ConnectionStrings": { "SafeBiteDb": "Server=<your-server>,1433;Database=SafeBiteDb;User Id=sa;Password=<your-password>;TrustServerCertificate=True" } }
   ```
4. Package Manager Console (default project SafeBite.Api): `Update-Database`
5. Run the API. On first start, `DbSeeder` fills the empty tables from `data/seed/` (100 establishments, 447 inspections, 100 owner responses).
6. Open `https://localhost:<port>/swagger` to test every endpoint. OpenAPI document: `/openapi/safebite.json`.

Main endpoints: `/api/establishments`, `/api/inspections`, `/api/owner-responses` (GetAll, GetById, POST, PUT, PATCH, DELETE each) and `/api/establishments/{id}/badge`.

### Run in Docker (Mac, builds a linux/arm64 image)

```bash
docker build --platform linux/arm64 -t safebite-api .
docker run --rm -p 8080:8080 -e 'ConnectionStrings__SafeBiteDb=Server=host.docker.internal,1433;Database=SafeBiteDb;User Id=sa;Password=<your-password>;TrustServerCertificate=True' safebite-api
```
Then open `http://localhost:8080/swagger`. Don't commit or share the command with your real password.

## Repo layout (planned)

```
SafeBiteTO/
  PROJECT.md          this file
  CLAUDE.md           context for Claude Code
  SafeBiteTO.slnx     Visual Studio solution
  data/raw/           raw DineSafe CSV (ignored by git)
  data/seed/          cleaned seed files (committed)
  SafeBite.Api/       ASP.NET Core Web API
  SafeBite.Client/    ASP.NET Core MVC client (later)
  docs/               task-definition.json, screenshots, report material
```

## Build order

1. Setup: IAM, AWS CLI, repo *(done by Isabella P.)*
2. Data: download DineSafe, pick about 100 establishments, clean into seed files *(done)*
3. Create the Web API project and install packages *(done)*
4. Models, DbContext, first migration on local SQL Server *(done)*
5. Seed real data *(done)*
6. Repositories, DTOs, AutoMapper profiles *(done)*
7. Three controllers with all six operations, JSON Patch *(done)*
8. OpenAPI and Swagger, test with Endpoints Explorer *(done: checkpoint, team can clone and run)*
9. Switch to RDS SQL Server
10. Dockerize *(done)*, push to ECR
11. ECS Fargate: task definition, Run task, save the JSON
12. Apigee setup and portal
13. MVC client calling all 6 methods per controller
14. Report, screenshots, demo practice, delete AWS resources

Build one controller end to end first, then repeat for the other two.

## Other details

- Ended up going w 100 establishments as opposed to the 20 required in the rubric because she mentioned that as a requirement in class.
- [ ] Whose AWS and Apigee accounts we use (screenshots must show the account)
- [ ] Swashbuckle.AspNetCore or .NET's built-in OpenAPI support

## Rules for the repo

- Never commit secrets: no AWS keys, RDS passwords or Apigee keys. Use user secrets locally and Parameter Store in AWS.
- Take screenshots as each cloud step works, with the AWS or Apigee account visible.
- Delete billable AWS resources after the screenshots (RDS, ECS tasks and clusters, load balancers, ECR images).
