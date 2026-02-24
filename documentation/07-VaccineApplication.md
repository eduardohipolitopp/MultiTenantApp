# MODULE 07 — VACCINE APPLICATION (CORE LOGIC)

## OBJECTIVE

Implement vaccine application process.

---

## DOMAIN

VaccineApplication:

- Id
- PatientId
- VaccineBatchId
- ApplicationDate
- DoseNumber
- ProfessionalId
- ApplicationType (Clinic, HomeVisit)
- PaidAmount
- AlertSent
- TenantId

Migration required.

---

## BUSINESS RULE

When saving VaccineApplication:

1. Decrease VaccineBatch.AvailableQuantity
2. Create Finance entry
3. Create next Appointment (DoseIntervalDays)
4. Persist everything in transaction

---

## APPLICATION

Service:
IVaccineApplicationService

Method:
ApplyVaccine()

Must:
- Use FIFO batch logic
- Validate stock
- Prevent negative quantity

---

## API

VaccineApplicationController

---

## UI

VaccineApplicationScreen

- Select Patient
- Select Vaccine
- Auto-select batch (FIFO)
- Confirm apply

---

## DONE

- Stock decreases
- Finance created
- Next appointment created
- All tenant safe