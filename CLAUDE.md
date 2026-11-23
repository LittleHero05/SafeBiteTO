# CLAUDE.md

Context for Claude Code in this repo. Read `PROJECT.md` first: it has the requirements, architecture, data model and build order.

## Project

SafeBite TO, a COMP306 (Centennial College) group project: an ASP.NET Core Web API (.NET 10) that serves food safety badges from Toronto's DineSafe data, deployed to ECS Fargate behind Google Apigee, with an ASP.NET Core MVC client.

## Environment

- Mac (Apple Silicon) with Parallels. Visual Studio runs in Windows on Parallels; the repo lives on the Mac and is shared into Windows (`\\Mac\Home\Documents\College\SEM6\SafeBiteTO`).
- Docker Desktop runs on the Mac. Docker images are built from the Mac's Terminal and are **linux/arm64**, so ECS task definitions use Linux/ARM64.
- Local database: SQL Server in a Docker container on the Mac. Cloud database: RDS for SQL Server.
- AWS CLI on the Mac, region `us-east-1`.

## Conventions

- .NET 10, C#, EF Core 10 **code first** with migrations.
- Repository pattern: controllers depend on repository interfaces.
- DTOs for every controller (Read, Create, Update at minimum), mapped with AutoMapper profiles. DTOs hide audit fields and flatten nested data.
- PATCH uses `JsonPatchDocument<T>` with `Microsoft.AspNetCore.JsonPatch` and `Microsoft.AspNetCore.Mvc.NewtonsoftJson`.
- A named OpenAPI document registered in Program.cs, with Swagger UI.
- Every controller implements GetAll, GetById, POST, PUT, PATCH, DELETE.
- Data must be real DineSafe data, not generated sample data (except owner responses).
- Never write secrets into the repo.

## How to help

- Guide step by step and explain what each step does. The team wants to understand and write the code themselves; don't make large changes without being asked.
- Follow the marking scheme in `PROJECT.md` closely.
