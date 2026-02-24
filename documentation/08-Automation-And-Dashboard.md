# MODULE 08 — AUTOMATION + DASHBOARD (HANGFIRE CORE)

## PROJECT CONTEXT

This module brings life to the system.

All previous modules created transactional data.

Now the system must:
- Think
- Monitor
- Alert
- Close commissions
- Detect delays
- Prevent vaccine expiration losses

All jobs must be implemented using Hangfire.

---

## DOMAIN

### NotificationQueue

- Id
- PatientId
- Channel (Email, SMS, WhatsApp)
- Template
- PayloadJson
- ScheduledSendDate
- SentDate
- Status (Pending, Sent, Failed)
- RetryCount
- TenantId

---

### CommissionMonthlyClosing

- Id
- ProfessionalId
- Month
- Year
- TotalApplications
- TotalHomeVisits
- CommissionAmount
- ClosedAt
- TenantId

---

### HomeVisitMonthlyClosing

- Id
- ProfessionalId
- Month
- Year
- TotalVisits
- BonusAmount
- ClosedAt
- TenantId

---

### DashboardDailySnapshot

- Id
- Date
- ApplicationsToday
- RevenueToday
- OverdueDoses
- ExpiringBatches
- ScheduledToday
- HomeVisitsMonth
- TenantId

---

## HANGFIRE JOBS

---

### DoseReminderJob

Daily at 02:00

Find future Appointments in next 3 days.

Create NotificationQueue entry.

---

### OverdueDoseJob

Daily at 02:10

Find missed appointments.

Create NotificationQueue.

---

### VaccineByAgeJob

Daily at 02:20

Check Patient age vs Vaccine.ApplicationAgeMonths.

Create recommendation message.

---

### BatchExpirationJob

Daily at 02:30

ExpirationDate <= 30 days.

Notify clinic.

---

### ExpiredBatchJob

Daily at 02:40

ExpirationDate < today.

Notify clinic.

---

### CommissionClosingJob

Monthly

Sum VaccineApplications grouped by Professional.

Insert CommissionMonthlyClosing.

---

### HomeVisitClosingJob

Monthly

Group ApplicationType HomeVisit.

Insert HomeVisitMonthlyClosing.

---

### DailyDashboardJob

Daily at 03:00

Generate DashboardDailySnapshot.

Cache in Redis.

---

### MessageRetryJob

Every 30 minutes.

Retry failed NotificationQueue.

---

## DASHBOARD SERVICE

Create IDashboardService.

Methods:
- GetTodayKPIs()
- GetMonthRevenue()
- GetOverdueDoses()
- GetExpiringBatches()

Must read from Redis snapshot.

---

## API

DashboardController.

---

## UI

DashboardScreen

Widgets:
- Today Applications
- Revenue Today
- Overdue Doses
- Expiring Batches
- Today's Schedule
- Monthly Commission
- Home Visits

---

## DEFINITION OF DONE

- Jobs registered in Hangfire
- Snapshot generated daily
- Dashboard loads from Redis
- Notifications created automatically
- Commission closing works