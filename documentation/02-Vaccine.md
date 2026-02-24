# MODULE 02 — VACCINE

## CONTEXT

Same architecture rules from Module 01 apply.

---

## OBJECTIVE

Implement Vaccine master data module.

---

## DOMAIN

Entity: Vaccine

Fields:
- Id
- Name
- Manufacturer
- ApplicationAgeMonths
- Doses
- DoseIntervalDays
- RequiresBooster
- Notes
- TenantId

Add configuration + migration.

---

## APPLICATION

DTOs:
- Create
- Update
- Read
- List

Validator:
- Name required
- Doses > 0

Service:
IVaccineService
VaccineService

CRUD
List with Redis cache

---

## API

VaccineController

---

## UI

VaccineScreen

- Full CRUD
- Pagination
- Validation messages

---

## DONE

- CRUD functional
- Tenant isolated
- Cache implemented