# MODULE 01 — PATIENT

## PROJECT CONTEXT

You are working inside the existing solution `MultiTenantApp`.

The architecture already exists and MUST NOT be modified.

Architecture:

- DDD
- Domain Layer
- Application Layer
- Infrastructure Layer
- API Layer
- Blazor Server (Web)
- EF Core
- ASP.NET Identity
- Multi-Tenancy (TenantId required)
- SoftDelete
- FluentValidation
- Redis caching
- Mongo Audit logging
- Hangfire

You must follow existing project patterns strictly.

Do NOT introduce new architectural styles.

---

## OBJECTIVE

Implement the full Patient module end-to-end.

---

## DOMAIN

Create entity:

Patient

Fields:
- Id (Guid)
- Name (string, required, max 100)
- BirthDate (Date)
- Gender (enum: Male, Female)
- GuardianName (string, optional)
- Phone (string, required)
- Email (string)
- Address (string)
- Notes (string)
- TenantId (Guid)

Requirements:
- Inherit BaseEntity
- Support SoftDelete
- Add IEntityTypeConfiguration
- Add DbSet
- Generate Migration

---

## APPLICATION

Create:

- PatientCreateDto
- PatientUpdateDto
- PatientReadDto
- PatientListDto

Create Validator:

PatientCreateValidator:
- Name required
- BirthDate cannot be future
- Phone required

Create:

IPatientService
PatientService

Methods:
- Create
- Update
- Delete (SoftDelete)
- GetById
- List (Tenant filtered)

List must use Redis Cache (5 min TTL).

---

## API

Create PatientController.

- Use IPatientService
- [Authorize]
- No business logic inside controller

---

## UI (Blazor)

Create PatientScreen.

Features:
- List with pagination
- Create modal
- Edit modal
- Soft delete

---

## DEFINITION OF DONE

- Builds without errors
- Migration generated
- CRUD works
- Tenant isolation enforced
- No DbContext usage outside repository layer