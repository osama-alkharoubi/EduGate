<div align="center">

# 🎓 EduGate

### Backend API for University Academic Management

*A RESTful backend for enrollment, academic structure, and end-of-semester grading — built for performance and academic integrity at scale.*

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Docker-336791?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean-informational?style=flat-square)]()
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)]()

</div>

---

## Overview

**EduGate** is a backend-only RESTful API that manages the full academic lifecycle of a university — from study plans and course enrollment to complex, end-of-semester bulk grading recalculations. It is built on **Clean Architecture** and **SOLID** principles, with a data access strategy split deliberately between **EF Core** (for domain CRUD) and **raw SQL via Dapper** (for high-throughput batch operations).

There is no frontend in this repository — this is a pure API service, documented and testable via **Scalar**. The centerpiece of the system is a **single atomic PostgreSQL CTE**, executed through Dapper, that recalculates GPA, academic standing, and graduation status for an entire university's student body in milliseconds.

---

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Security & RBAC](#security--rbac)
- [Core Features](#core-features)
- [The Grading Engine](#the-high-performance-grading-engine)
- [Academic Integrity Guards](#academic-integrity--guard-clauses)
- [Running the Project](#running-the-project)
- [Project Structure](#project-structure)
- [License](#license)

---

## Architecture

EduGate follows **Clean Architecture**, enforcing a strict, one-directional dependency flow:

```
┌─────────────────────────────────────────────┐
│                Presentation                  │  ← API Controllers, Scalar docs
├─────────────────────────────────────────────┤
│                Infrastructure                │  ← EF Core, Dapper, external services
├─────────────────────────────────────────────┤
│                 Application                  │  ← Use cases, DTOs, validation
├─────────────────────────────────────────────┤
│                    Domain                    │  ← Entities, business rules
└─────────────────────────────────────────────┘
```

Business rules live in the **Domain** layer with zero dependency on infrastructure. Outer layers depend inward, never the reverse — keeping the grading engine, enrollment rules, and academic policy testable in isolation from the database and the API framework.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language / Runtime | C# on **.NET 10** |
| Database | **PostgreSQL**, containerized via **Docker** |
| ORM | **Entity Framework Core** — domain CRUD, simple queries |
| High-performance data access | **Dapper** — raw SQL for bulk operations (grading engine) |
| Auth | **JWT** with **Role-Based Access Control (RBAC)** |
| API Documentation | **Scalar** |
| Testing | **xUnit** + **Moq** |

**Why two data access tools?** EF Core gives clean, maintainable, type-safe CRUD for everyday domain operations. But recalculating GPA, academic warnings, and graduation status for thousands of students at semester-end is not a CRUD problem — it's a set-based batch computation. Doing that through an ORM means materializing thousands of tracked entities into memory. Dapper + a single CTE lets PostgreSQL do what it's best at: process the whole operation set-at-a-time, atomically, in the database itself.

---

## Security & RBAC

Access is strictly isolated by role at the API layer:

| Role | Permissions |
|---|---|
| **Admin / Registrar** | Finalize semesters, trigger bulk GPA recalculation, manage academic statuses |
| **Professor** | Submit grades — restricted to sections they are assigned to teach |
| **Student** | View their own enrollments, grades, and academic standing |

---

## Core Features

### 📚 Academic Structure & Study Plans
- Manages specializations and degree requirements
- Maps courses to curriculum study plans
- Tracks student progress and completed credits against exact degree requirements

### 📝 Enrollment & Section Management
- Manages Courses, Sections, Semesters, and Enrollments
- Validates section capacity and active-semester constraints
- Enforces specialization eligibility and prerequisite completion before allowing enrollment

### 🎯 Grading System
- Professors submit bulk grades for their assigned sections
- Automatic passing evaluation (grade ≥ 50 → `Completed`, otherwise `Failed`)

---

## The High-Performance Grading Engine

The core technical achievement of EduGate. At end-of-semester finalization, instead of loading thousands of enrollment records into memory through an ORM, a **single atomic CTE executed via Dapper** processes the entire university's grades in one pass:

- **Semester GPA & Credits** — weighted average calculated per student for the current term
- **Cumulative GPA** — computed across all terms; when a course is retaken, the engine automatically partitions the data and selects only the **highest grade** for the cumulative calculation
- **Academic Warning System** — cumulative GPA below 60 triggers an automatic warning (capped at 3)
- **Academic Dismissal** — 3 accumulated warnings with GPA still below 60 → status automatically set to `Dismissed`
- **Graduation Processing** — completed credits are compared against the specialization's required credits; students with 12–21 remaining credits are flagged `IsGraduating`, and students with all required credits complete are marked `Graduated`

All of this runs as one atomic operation — no partial state, no N+1 queries, no per-student round trips.

---

## Academic Integrity & Guard Clauses

Two safeguards protect the correctness of every finalization run:

- **Incomplete Grade Handling** — a pending `Incomplete` (Status = 6) in any course **freezes** that student's warning counter and academic status, preventing unfair warnings or dismissal while the grade is unresolved.
- **Semester Finalization Guard Clause** — the Registrar **cannot** finalize a semester while even one active enrollment has no submitted grade. Bulk calculations never run against incomplete data.

---

## Running the Project

### 1. Prerequisites

Make sure the following are installed before you start:

| Tool | Purpose |
|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Build and run the API |
| [Docker](https://www.docker.com/) & Docker Compose | Run PostgreSQL in a container |
| [EF Core CLI tools](https://learn.microsoft.com/ef/core/cli/dotnet) | Apply database migrations |
| A REST client (Scalar UI, Postman, or `curl`) | Call the API |

Install the EF Core CLI tool if you don't already have it:

```bash
dotnet tool install --global dotnet-ef
```

### 2. Clone the repository

```bash
git clone https://github.com/osama-alkharoubi/edugate.git
cd edugate
```

### 3. Start PostgreSQL via Docker

```bash
docker compose up -d
```

This spins up a PostgreSQL container using the settings in `docker-compose.yml`. Confirm it's running:

```bash
docker ps
```

### 4. Configure the connection string and JWT settings

Set these in `src/EduGate.Presentation/appsettings.Development.json`, or as environment variables (recommended for the JWT secret):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=edugate;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-secret-key",
    "Issuer": "EduGate",
    "ExpiryMinutes": 60
  }
}
```

### 5. Apply EF Core migrations

```bash
dotnet ef database update \
  --project src/EduGate.Infrastructure \
  --startup-project src/EduGate.Presentation
```

This creates the schema (Students, Courses, Sections, Enrollments, etc.) in the containerized database.

### 6. Run the API

```bash
dotnet run --project src/EduGate.Presentation
```

The API will start on the port defined in `launchSettings.json` (typically `https://localhost:7xxx`).

### 7. Explore the API

Open the Scalar documentation UI in your browser:

```
https://localhost:{port}/scalar
```

From there you can authenticate (JWT), browse every endpoint grouped by role, and send test requests directly from the browser.

### 8. Run the test suite

```bash
dotnet test
```

Runs the full xUnit + Moq suite covering domain rules, enrollment eligibility logic, and the grading engine.

### Troubleshooting

| Problem | Fix |
|---|---|
| `dotnet ef` command not found | Run `dotnet tool install --global dotnet-ef`, then restart your terminal |
| Migration fails to connect | Confirm the Docker container is running (`docker ps`) and the connection string matches the exposed port |
| 401 on protected endpoints | Obtain a JWT via the auth endpoint first, then pass it as `Authorization: Bearer {token}` |
| Port already in use | Change the port in `docker-compose.yml` or `launchSettings.json` |

---

## Project Structure

```
EduGate/
├── src/
│   ├── EduGate.Domain/           # Entities, enums, core business rules
│   ├── EduGate.Application/      # Use cases, DTOs, interfaces, validation
│   ├── EduGate.Infrastructure/   # EF Core, Dapper, repositories, migrations
│   └── EduGate.Presentation/     # API controllers, middleware, configuration
├── tests/
│   └── EduGate.Tests/            # xUnit + Moq test suite
├── docker-compose.yml
└── README.md
```

---

## License

Distributed under the MIT License. See `LICENSE` for details.

---

<div align="center">

Built by [Osama Alkharoubi](https://github.com/osama-alkharoubi)

</div>
