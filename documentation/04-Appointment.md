# MODULE 04 — APPOINTMENT

## OBJECTIVE

Implement vaccine scheduling.

---

## DOMAIN

Appointment:

- Id
- PatientId
- VaccineId
- ProfessionalId
- ScheduledDateTime
- Status (Scheduled, Applied, Cancelled)
- Type (Clinic, HomeVisit)
- TenantId

Migration required.

---

## APPLICATION

Service:
IAppointmentService

CRUD

List by:
- Date
- Patient
- Vaccine

---

## API

AppointmentController

---

## UI

AppointmentScreen

Calendar:
- Day
- Week
- Month

---

## DONE

- Calendar renders
- Filtering works
- Tenant enforced