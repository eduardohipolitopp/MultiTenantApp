# MODULE 09 — SYSTEM GOVERNANCE + SECURITY + PERMISSIONS

## PROJECT CONTEXT

This module hardens the system for production.

All business modules are already implemented.

Now we must ensure:

- Security
- Role-based access
- Permission policies
- Audit integrity
- Tenant isolation enforcement
- System configuration
- Data protection rules

This module must follow existing Identity configuration.

Do NOT replace Identity.
Extend it properly.

---

# PART 1 — ROLES

Create predefined roles:

- Admin
- Manager
- Nurse
- Receptionist
- Financial

Use ASP.NET Identity Roles.

Seed them during startup.

---

# PART 2 — PERMISSIONS (POLICY-BASED)

Define Policies for:

PATIENT:
- Patient.View
- Patient.Create
- Patient.Edit
- Patient.Delete

VACCINE:
- Vaccine.View
- Vaccine.Create
- Vaccine.Edit

STOCK:
- Stock.View
- Stock.Create
- Stock.Edit

APPOINTMENT:
- Appointment.View
- Appointment.Create
- Appointment.Edit
- Appointment.Cancel

APPLICATION:
- Application.Apply

FINANCE:
- Finance.View
- Finance.CloseMonth

DASHBOARD:
- Dashboard.View

ADMIN:
- Settings.Edit
- User.Manage

Map roles to policies:

Admin → All
Manager → All except User.Manage
Nurse → Application.Apply, Patient.View, Appointment.View
Receptionist → Patient.*, Appointment.*
Financial → Finance.*, Dashboard.View

---

# PART 3 — CLINIC SETTINGS

Create entity:

ClinicSettings

Fields:
- Id
- ClinicName
- CommissionPercentage
- HomeVisitBonus
- ReminderDaysBefore
- ExpirationAlertDays
- DefaultCurrency
- TenantId

Must be cached in Redis.

Create:

IClinicSettingsService

Methods:
- GetSettings
- UpdateSettings

---

# PART 4 — PROFESSIONAL LINK

Extend Professional entity:

- IdentityUserId

Ensure:
Only linked professionals can apply vaccines.

---

# PART 5 — CRITICAL VALIDATIONS

Implement in Application layer:

- Prevent applying expired batch
- Prevent negative stock
- Prevent duplicate dose number for same patient
- Prevent deleting patient with applications
- Prevent double monthly closing
- Prevent editing batch after usage

All must throw domain exceptions.

---

# PART 6 — AUDIT EXTENSION

Every Create, Update, Delete must:

- Log UserId
- Log Timestamp
- Log EntityName
- Log OperationType

Reuse Mongo audit infrastructure.

---

# PART 7 — TENANT ENFORCEMENT

Add global query filter:

builder.Entity().HasQueryFilter(e => e.TenantId == _currentTenant)

Validate:
No service can bypass tenant filter.

---

# PART 8 — ADMIN UI

Create screens:

1. Role Management
2. User Management
3. Clinic Settings
4. Permissions View Matrix

Only Admin role can access.

---

# PART 9 — SEED DATA

On startup:

- Create default Admin user if none exists
- Create default ClinicSettings
- Create Roles
- Assign Admin role

---

# PART 10 — PRODUCTION HARDENING

Ensure:

- All endpoints use [Authorize]
- Sensitive endpoints use policies
- No controller exposes internal entities
- No DbContext injected in controller

---

# DEFINITION OF DONE

- Roles seeded
- Policies enforced
- Settings configurable
- Tenant isolation guaranteed
- Critical validations working
- Audit working
- Admin UI functional
- No unauthorized access possible