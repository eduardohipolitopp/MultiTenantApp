# MODULE 03 — VACCINE BATCH (STOCK)

## OBJECTIVE

Implement stock control by batch.

---

## DOMAIN

Entity: VaccineBatch

Fields:
- Id
- VaccineId
- BatchNumber
- TotalQuantity
- AvailableQuantity
- ExpirationDate
- EntryDate
- Supplier
- Notes
- TenantId

Migration required.

---

## RULE

Stock must support FIFO by ExpirationDate.

---

## APPLICATION

DTOs

Validator:
- BatchNumber required
- ExpirationDate required

Service:
IVaccineBatchService

Methods:
- Create
- Update
- GetById
- List
- GetNextAvailableBatchFIFO(vaccineId)

List must use Redis cache.

---

## API

VaccineBatchController

---

## UI

VaccineBatchScreen

- Batch entry form
- Batch listing
- Expiration highlight

---

## DONE

- FIFO logic implemented
- Tenant safe
- No direct DbContext access