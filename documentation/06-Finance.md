# MODULE 06 — FINANCE

## OBJECTIVE

Implement financial tracking.

---

## DOMAIN

Finance:

- Id
- PatientId
- ProfessionalId
- VaccineId
- Amount
- Type (Clinic, HomeVisit, Sale)
- PaymentDate
- CommissionCalculated
- TenantId

---

## APPLICATION

Service:
IFinanceService

Methods:
- RegisterPayment
- CalculateCommission
- MonthlyClosing

---

## API

FinanceController

---

## UI

FinanceScreen

- Register payment
- Commission summary
- Monthly closing

---

## DONE

- Commission calculation working
- Tenant isolated