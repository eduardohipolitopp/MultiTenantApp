# MODULE 05 — MESSAGE

## OBJECTIVE

Implement messaging system.

---

## DOMAIN

Message:

- Id
- Recipient
- Channel (SMS, WhatsApp, Email)
- Template
- Content
- SentDate
- Status (Pending, Sent, Failed)
- PatientId
- TenantId

---

## APPLICATION

Service:
IMessageService

Methods:
- Create
- Send
- Retry
- List

---

## API

MessageController

---

## UI

MessageScreen

- Template support:
  {PatientName}
  {Vaccine}
  {DoseNumber}
  {ScheduledDate}

- Retry failed

---

## DONE

- Message persisted
- Status updated