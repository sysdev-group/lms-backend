# MODERN MODULAR

## LEARNING MANAGEMENT SYSTEM

### Full System Documentation

###### Version 2.0 | 23 March 2026


## Table of Contents

**1 Project Overview**

**2 Stakeholders**

**3 System Requirements**

**4 Technology Stack**

**5 System Architecture**

**6 Database Architecture**

**7 Core System Modules**

```
7.1 Authentication Module
7.2 Authorization Module
7.3 User Management Module
7.4 Course Module
7.5 Assignment Module
7.6 Grading Module
7.7 Notification System
7.8 Messaging System
7.9 Dashboard Module
```
**8 Security Design**

**9 UI/UX Design Principles**

**10 Code Quality Strategy**

**11 Scalability Strategy**

**12 Error Handling Strategy**

**13 Performance Optimization**

**14 Maintainability Design**

**15 Testing Strategy**

**16 Deployment Strategy**

**17 Future Enhancements**

**18 Risks and Mitigation**

**19 Success Criteria**

**21 File Upload & Storage Architecture**

**22 Turnitin Integration**

**23 Multi-Channel Notification System**

**24 Timetable Management Module**

**_— Supplementary Specifications (New) —_**

**25 API Design & Contract Documentation**

**26 Attendance Management Module**

**27 Grade Calculation & Assessment Structure**

**28 Enrollment Workflow**

**29 Password Reset & Account Recovery**

**30 JWT Refresh Token Strategy**

**31 Search & Filtering System**

**32 Audit Log Module**

**33 File Download Tracking**

**34 Semester & Academic Calendar Module**

**35 Assignment Rubrics & Marking Criteria**


**36 Student Progress Tracking**

**37 Rate Limiting & Abuse Protection**

**38 Backup & Recovery Plan**

**39 Localization & Multi-Language Support**


## 1. Project Overview

#### 1.1 System Name

Modern Modular Learning Management System

#### 1.2 Purpose

This system is designed to replace an existing Moodle-based LMS with a modern, scalable, user-friendly
platform that improves usability, performance, maintainability, and extensibility.

#### 1.3 Objectives

The system is designed to:

- Provide fast and reliable UI/UX
- Reduce navigation complexity
- Improve user productivity
- Ensure clean code architecture
- Enable scalability
- Maintain high security standards
- Allow future expansion

#### 1.4 Scope

The system supports student learning activities, lecturer course management, and administrative system
control. The platform will be web-based and responsive.


## 2. Stakeholders

```
Stakeholder Interest
```
```
Students Access materials, submit work
```
```
Lecturers Manage courses and grading
```
```
Administrators Manage system operations
```
```
Institution Reliable LMS platform
```

## 3. System Requirements

#### 3.1 Functional Requirements

##### Student

- Login / logout
- View courses
- Submit assignments
- View grades
- Receive notifications
- Download resources

##### Lecturer

- Create assignments
- Upload materials
- Grade submissions
- Manage students
- Post announcements

##### Admin

- Manage users
- Manage roles
- Create courses
- Send bulk messages
- View system reports

#### 3.2 Non-Functional Requirements

```
Category Requirement
```
```
Performance Dashboard loads < 2 seconds; API response < 500ms average
```
```
Security Encrypted passwords, JWT authentication, role-based authorization
```
```
Usability Clean layout, mobile responsive, consistent UI
```
```
Reliability System uptime ≥ 99%; automatic backups
```
```
Maintainability Modular codebase; separation of concerns
```

## 4. Technology Stack

```
Layer Technology
```
```
Frontend Angular, RxJS, Angular Router, Angular Material / Tailwind CSS
```
```
Backend ASP.NET Core Web API — Clean Architecture Pattern
```
```
Primary Database PostgreSQL
```
```
Future Database MongoDB (logs, messages, notifications)
```

## 5. System Architecture

The system follows a layered clean architecture:

```
Layer 1 Client — Angular SPA
```
```
Layer 2 API Controllers
```
```
Layer 3 Application Services
```
```
Layer 4 Domain Layer
```
```
Layer 5 Infrastructure Layer
```
```
Layer 6 Database
```
#### Architectural Principles

- Clean Architecture
- Separation of Concerns
- Dependency Injection
- Modular Design


## 6. Database Architecture

#### Current Implementation

Single relational database (PostgreSQL). LMS data is relational and structured, making PostgreSQL the ideal
primary store.

#### Future Scalability Plan

The system supports adding a NoSQL database (MongoDB) for notifications, logs, messaging, and event
tracking. This follows the Polyglot Persistence principle. The abstraction layers in place ensure this migration
will not affect application logic.


## 7. Core System Modules

#### 7.1 Authentication Module

Handles all identity verification for system access.

```
Function Description
```
```
Login Validates credentials and issues JWT access token
```
```
Logout Revokes session and invalidates token
```
```
Token Validation Verifies JWT signature and expiry on each request
```
```
Session Expiration Automatic expiry with re-authentication prompt
```
```
Security: hashed passwords, token expiry, secure authentication endpoints
```
#### 7.2 Authorization Module

Role-Based Access Control (RBAC) governs what each user can see and do. Access is validated at both API
level and UI level.

```
Role Access Level
```
```
Student Personal data, enrolled course content, own submissions
```
```
Lecturer Their courses, students, submissions, grading
```
```
Admin Full system access
```
#### 7.3 User Management Module

Administrators manage all user accounts through this module.

- Create users
- Assign and change roles
- Deactivate accounts
- Reset passwords
- Bulk import users via CSV

#### 7.4 Course Module

- Create and configure courses
- Enroll students
- Assign lecturers
- Archive completed courses

#### 7.5 Assignment Module

- Create assignments with deadlines


- Upload submission files
- Track deadlines and late submissions
- Allow resubmission (configurable)
- Detect and flag late submissions

#### 7.6 Grading Module

- Input and update grades
- Feedback system per submission
- Grade visibility toggle — hidden until published
- Export results to CSV

#### 7.7 Notification System

Provides in-app alerts, course announcements, and deadline reminders with read/unread tracking and priority
levels (Normal / Important / Urgent).

#### 7.8 Messaging System

Admins and lecturers can send bulk messages, targeted messages, and scheduled messages to defined
recipient groups.

#### 7.9 Dashboard Module

Each role has a fully customised dashboard:

```
Role Dashboard Contents
```
```
Student Upcoming deadlines, announcements, enrolled courses, progress indicators
```
```
Lecturer Pending grading queue, course shortcuts, today's schedule
```
```
Admin System stats, user metrics, recent activity feed
```

## 8. Security Design

Security is enforced at every layer of the application stack:

- JWT authentication with short-lived access tokens
- Role-based authorization validated on every API endpoint
- Password hashing using bcrypt
- File upload validation — type and size restrictions enforced server-side
- Input sanitization to prevent injection attacks
- API request validation using model binding and data annotations
- All sensitive endpoints protected with HTTPS


## 9. UI/UX Design Principles

```
Principle Application
```
```
Clarity Actions and labels are unambiguous; no jargon
```
```
Simplicity Minimal steps to complete common tasks
```
```
Minimalism No unnecessary UI elements; clean whitespace
```
```
Consistency Uniform component styles and interaction patterns across all pages
```
```
Accessibility WCAG 2.1 AA compliant — keyboard navigation, screen reader support
```
```
Visual Hierarchy Important content and actions receive prominent placement
```

## 10. Code Quality Strategy

- Modular components with single responsibilities
- Reusable services shared across modules
- Short, well-named methods — max ~30 lines per method
- Meaningful naming conventions — no abbreviations
- No duplicated logic — shared utilities extracted to helpers
- Layered architecture enforced — no cross-layer shortcuts


## 11. Scalability Strategy

- Modular backend services — each module independently deployable
- Database indexing on frequently queried fields
- Pagination on all list endpoints
- Caching for frequent read-heavy data (timetables, course lists)
- Load balancing ready — stateless API design
- Microservice transition capability — modules can be extracted


## 12. Error Handling Strategy

- Centralised exception handling middleware in ASP.NET Core
- Standardised error response format (see Section 25 for envelope spec)
- Structured logging with severity levels (Info, Warning, Error, Critical)
- Client-facing validation feedback — field-level error messages
- No raw stack traces returned to clients in production


## 13. Performance Optimization

- Lazy loading Angular modules — only load what is needed
- API pagination on all list endpoints
- Query optimization — select only required fields, avoid N+1 queries
- Caching for frequently accessed, rarely changing data
- Minimised network calls — batch related data in single responses


## 14. Maintainability Design

- Strict separation of layers — UI, Application, Domain, Infrastructure
- Clean folder structure mirroring module boundaries
- Dependency injection throughout — no tight coupling
- Inline XML documentation on all public API methods
- Reusable shared modules — no copy-paste coding


## 15. Testing Strategy

```
Test Type Scope Tooling
```
```
Unit Tests Individual services and domain
logic
```
```
xUnit (backend), Jasmine/Karma
(frontend)
```
```
API Tests All REST endpoints — success
and error cases
```
```
Postman / REST Assured
```
```
Integration Tests Module interactions and database
operations
```
```
xUnit + TestContainers
```
```
User Acceptance Tests End-to-end workflows per role Manual / Cypress
```

## 16. Deployment Strategy

- Cloud hosting (Azure / AWS — provider TBD)
- Docker containerisation — all services containerised
- CI/CD pipeline — automated build, test, and deploy on merge to main
- Environment configurations — Development, Staging, Production
- Zero-downtime deployment strategy using rolling updates


## 17. Future Enhancements

```
Feature Description
```
```
AI Academic Assistant Conversational assistant for students to get help with coursework
```
```
Plagiarism Detection Deep integration with Turnitin or Copyleaks
```
```
Attendance Analytics Predictive alerts when attendance drops below threshold
```
```
Performance Predictions ML model to identify at-risk students early
```
```
Mobile Application Native iOS and Android apps using the existing API
```
```
Offline Mode Service worker caching for timetable and materials access offline
```
```
Drag-and-Drop Timetable Visual schedule builder for administrators
```
```
Automatic Timetable Generator AI-assisted conflict-free schedule generation
```

## 18. Risks and Mitigation

```
Risk Likelihood Mitigation
```
```
System complexity grows
unmanageable
```
```
Medium Modular architecture; strict coding standards
```
```
Performance degradation at
scale
```
```
Medium Caching, indexing, pagination from day one
```
```
Security breach or data
exposure
```
```
Low RBAC, input validation, auditing, HTTPS
```
```
Database scaling limits Low Polyglot persistence plan; indexing strategy
```
```
Third-party API unavailability
(Turnitin)
```
```
Medium Graceful fallback; queue-based async calls
```
```
Developer turnover Medium Comprehensive documentation; clean codebase
```

## 19. Success Criteria

The system is considered successful when all of the following are met:

- Users can find any piece of information within 3 clicks from their dashboard
- The UI measurably reduces confusion compared to the legacy Moodle system (validated by UAT)
- Lecturers report time savings in grading and course management workflows
- System meets ≥ 99% uptime target over a 90-day production window
- Codebase passes peer review for maintainability and passes all automated tests


## 21. File Upload & Storage Architecture

#### 21.1 Purpose

The File Storage Module handles all file operations including assignment submissions, lecture materials,
attachments, profile images, and downloadable resources.

#### 21.2 Supported File Types

```
Category Formats
```
```
Documents PDF, DOCX, PPTX
```
```
Images JPG, PNG
```
```
Archives ZIP
```
```
Code Files .py, .java, .cs, .js, .ts
```
```
Media (optional) MP4
```
#### 21.3 Upload Workflow

- User selects file in the Angular UI
- Frontend validates file size and type before upload
- File is sent to the API upload endpoint
- Backend re-validates type, size, and filename
- File is stored on disk or cloud storage
- Metadata is saved to the Files database table
- Success response returned to client

#### 21.4 Storage Strategy

##### MVP — Local Storage

```
Files stored on server disk under /uploads/{courses|assignments|submissions|profiles}
```
Simple, fast, and easy to debug — ideal for MVP phase.

##### Production — Cloud Storage (Future)

Architecture supports switching to AWS S3, Azure Blob Storage, or Google Cloud Storage via a pluggable
FileService abstraction layer.

#### 21.5 Files Metadata Table

```
Field Description
```
```
Id Unique identifier
```
```
FileName Stored (sanitised) filename
```
```
OriginalName User's original filename
```

```
Path Storage path or cloud URL
```
```
Size File size in bytes
```
```
UploadedBy User ID of uploader
```
```
UploadDate Timestamp (UTC)
```
```
RelatedEntity course / assignment / submission / profile
```
```
EntityId ID of the related record
```
#### 21.6 Security Measures

- Allowed file type whitelist enforced server-side
- Maximum file size limit (configurable — default 25MB)
- Filename sanitisation — removes path traversal characters
- Files stored outside public web root — never directly accessible via URL
- Download endpoints require valid authentication token
- Virus scan hook supported for future integration

#### 21.7 Access Control

```
Role File Access
```
```
Student Own submissions + course materials for enrolled courses only
```
```
Lecturer All files from their own courses
```
```
Admin All files system-wide
```
#### 21.8 Performance & Scalability

- Large files streamed — not loaded into memory
- Caching headers applied to static course resources
- Storage abstraction layer supports CDN integration
- Horizontal scaling supported — files decoupled from application server


## 22. Turnitin Integration (Plagiarism & AI Writing

## Detection)

#### 22.1 Purpose

Integrate Turnitin to automatically check student submissions for plagiarism/similarity scores, AI writing
detection, and originality reports accessible to lecturers and administrators.

#### 22.2 Roles & Permissions

```
Role Actions
```
```
Student Submit assignment normally; view report status; view report only if lecturer permits
```
```
Lecturer Configure Turnitin per assignment; view similarity/AI indicators; open originality
report
```
```
Admin Configure integration keys; enable/disable Turnitin; set defaults; view audit logs
```
#### 22.3 Integration Methods

##### Option A — LTI 1.3 (Recommended)

Standard Learning Tools Interoperability protocol. Supports deep linking into Turnitin UI with less custom API
work and better long-term compatibility.

##### Option B — Turnitin API

Backend uploads submissions and retrieves report data via Turnitin REST endpoints. Provides fully custom
UX but requires API access agreement.

#### 22.4 Submission Workflow

- Student uploads assignment file (PDF/DOCX)
- LMS stores file and submission metadata
- LMS sends submission to Turnitin asynchronously via background job
- Turnitin processes report (can take several minutes)
- LMS stores results: similarity %, AI indicator, report link, status
- Lecturer views report from Gradebook

#### 22.5 Assignment Settings (Lecturer Configuration)

- Enable / disable Turnitin check per assignment
- Enable AI writing detection (if supported by institution's plan)
- Allow students to view their own report (Yes / No)
- Exclude bibliography and quoted sections
- Set minimum match threshold (optional)


#### 22.6 TurnitinReports Table

```
Field Description
```
```
Id Unique report ID
```
```
SubmissionId Link to LMS submission record
```
```
TurnitinSubmissionId Provider's reference ID
```
```
SimilarityScore Percentage similarity (0–100)
```
```
AiWritingIndicator Yes / No / Unknown
```
```
Status Pending / Processing / Complete / Error
```
```
ReportUrl Secure link to originality report
```
```
CreatedAt / UpdatedAt Timestamps (UTC)
```
#### 22.7 Background Processing

Turnitin checks are processed asynchronously using Hangfire (ASP.NET Core) for MVP, with migration path
to RabbitMQ or Azure Service Bus for production scale. This keeps the UI responsive regardless of Turnitin
processing time.

#### 22.8 Alternative Providers

If Turnitin access is unavailable, the system architecture supports Copyleaks (AI detection + plagiarism),
Ouriginal, or PlagScan as drop-in alternatives.


## 23. Multi-Channel Notification System

#### 23.1 Purpose

Targeted messaging system allowing administrators to send notifications to specific user groups based on
academic structure. Supports in-app, email, and SMS delivery channels.

#### 23.2 Message Composition

```
Field Description
```
```
Title Short message title (required)
```
```
Body Full message content (required)
```
```
Priority Normal / Important / Urgent
```
```
Delivery Channels In-App, Email, SMS (multi-select)
```
```
Schedule Time Optional — send at future date/time
```
```
Expiry Date Optional — hide message after this date
```
#### 23.3 Recipient Targeting

Admin filters recipients using academic hierarchy:

```
Faculty Target all users in a faculty
```
```
Program Target all users in a program
```
```
Batch / Intake Target a specific intake year
```
```
Course Target students in a specific course
```
```
Individual Users Override for specific users
```
Filters can be combined. Example: Faculty = Computing AND Batch = 2023 sends only to that cohort.

#### 23.4 Academic Hierarchy

Faculty → Program → Batch → Student. This model enables fast, accurate recipient resolution.

#### 23.5 Lecturer Restrictions

Lecturers can only message students enrolled in their own courses. They cannot send institution-wide
messages or target other programs.

#### 23.6 Delivery & Scalability

- Recipient resolution calculated server-side — never exposed to client
- Background service dispatches messages in chunks


- Queue-based dispatch for large recipient lists
- Delivery results logged per recipient
- Spam burst prevention — rate limiting applied to message sending


## 24. Timetable Management Module

#### 24.1 Purpose

Replaces external scheduling tools by providing an integrated scheduling system. Admins create, manage,
and publish academic timetables with built-in conflict detection and real-time notifications.

#### 24.2 Academic Hierarchy

Faculty → Program → Batch → Semester → Sessions

#### 24.3 Admin Features

- Create timetable for Faculty / Program / Batch / Semester
- Add, edit, and delete sessions
- Duplicate timetable for new semester
- Import timetable via CSV (future enhancement)
- Publish draft timetable — triggers notifications

#### 24.4 Session Fields

```
Field Description
```
```
Module Subject or module name
```
```
Lecturer Assigned lecturer
```
```
Room Classroom or lab
```
```
Day Day of the week
```
```
Start Time / End Time Session duration
```
```
Type Lecture / Lab / Tutorial
```
#### 24.5 Conflict Detection Engine

System validates each session before saving. Conflicts detected:

- Lecturer overlap — same lecturer assigned at the same time
- Room overlap — same room double-booked
- Batch overlap — same batch scheduled in two sessions simultaneously

On conflict: system blocks save, highlights conflicting entries, and displays a clear conflict message.

#### 24.6 Publishing States

```
Status Description
```
```
Draft Editable — not visible to students or lecturers
```

```
Published Visible to all relevant users; notifications optionally sent
```
#### 24.7 Student View

- Weekly timetable view
- Today's schedule view
- Module filter
- Session detail popup
- Offline access — timetable cached locally; syncs on reconnect

#### 24.8 Lecturer View

- Personal weekly schedule
- Today's classes
- Upcoming sessions
- Attendance marking shortcut from session entry

#### 24.9 Database Schema

```
Table Key Fields
```
```
Timetables Id, FacultyId, ProgramId, BatchId, SemesterId, Status, CreatedBy,
CreatedAt
```
```
TimetableSessions Id, TimetableId, ModuleId, LecturerId, RoomId, DayOfWeek, StartTime,
EndTime, Type
```

#### SUPPLEMENTARY SPECIFICATIONS

_Sections 25–39 define features and requirements identified as missing from the original documentation._


## 25. API Design & Contract Documentation

**Priority: Critical — Must be agreed before development begins**

#### 25.1 Purpose

Defines the API surface, versioning strategy, and response contracts that all endpoints must follow to ensure
frontend/backend consistency.

#### 25.2 Base URL & Versioning

```
Development Base URL https://localhost:5001/api/v1
```
```
Production Base URL https://<domain>/api/v1
```
```
Version Strategy URI versioning — /api/v1/, /api/v2/
```
```
Content Type application/json
```
```
Authentication Bearer token in Authorization header
```
#### 25.3 Standard Response Envelope

Every API response must use this envelope:

```
Field Type Description
```
```
success boolean true for 2xx responses, false for errors
```
```
data object | array | null Response payload — null on error
```
```
message string Human-readable status message
```
```
errors array | null Field-level validation errors — null on success
```
```
pagination object | null Present only on paginated list responses
```
#### 25.4 HTTP Status Code Standards

```
Code Usage
```
```
200 OK Successful GET, PUT, PATCH
```
```
201 Created Successful POST creating a resource
```
```
204 No Content Successful DELETE
```
```
400 Bad Request Validation failure — include errors array
```
```
401 Unauthorized Missing or invalid token
```
```
403 Forbidden Valid token but insufficient role
```
```
404 Not Found Resource does not exist
```
```
409 Conflict Duplicate resource (e.g. already enrolled)
```

```
422 Unprocessable
Entity
```
```
Business rule violation (e.g. past deadline)
```
```
429 Too Many Requests Rate limit exceeded
```
```
500 Internal Server Error Unhandled server exception
```
#### 25.5 Core Endpoint Groups

```
Module Base Route Key Endpoints
```
```
Auth /api/v1/auth POST /login, POST /logout, POST /refresh, POST /forgot-
password
```
```
Users /api/v1/users GET /, POST /, GET /{id}, PUT /{id}, DELETE /{id}
```
```
Courses /api/v1/courses GET /, POST /, GET /{id}, GET /{id}/students
```
```
Assignments /api/v1/assignments GET /, POST /, GET /{id}, PUT /{id}/grade
```
```
Submissions /api/v1/submissions POST /, GET /{id}, GET /assignment/{id}
```
```
Grades /api/v1/grades GET /course/{id}, GET /student/{id}
```
```
Files /api/v1/files POST /upload, GET /{id}/download
```
```
Notifications /api/v1/notifications GET /, POST /, PATCH /{id}/read
```
```
Timetable /api/v1/timetable GET /batch/{id}, POST /, PUT /{id}/publish
```
```
Attendance /api/v1/attendance POST /session/{id}, GET /student/{id}
```
```
Search /api/v1/search GET /?q={query}&type={type}
```
```
Audit /api/v1/audit GET /?entity={type}&from={date}&to={date}
```
#### 25.6 OpenAPI / Swagger

The backend must expose Swagger UI at /swagger in non-production environments. All controllers must
include XML documentation comments for auto-generated API spec.


## 26. Attendance Management Module

**Priority: Critical — Core academic feature referenced but never specified**

#### 26.1 Purpose

Allows lecturers to record student attendance for each scheduled session. Provides students, lecturers, and
administrators with attendance visibility and reporting.

#### 26.2 Roles & Permissions

```
Role Actions
```
```
Student View own attendance record and percentage per module
```
```
Lecturer Mark attendance for sessions in their assigned modules
```
```
Admin View all attendance, override records, export reports
```
#### 26.3 Attendance Statuses

```
Status Code Description
```
```
Present P Student attended the session
```
```
Absent A Student did not attend
```
```
Late L Arrived after start but within grace period
```
```
Excused E Absence approved by lecturer or admin
```
```
Not Taken N/A Lecturer did not record attendance
```
#### 26.4 Marking Workflow

- Lecturer opens the session from their timetable
- Selects "Mark Attendance" — system pre-populates the enrolled student list
- Lecturer marks each student P / A / L / E
- Submits — system timestamps the record
- Records immediately visible to students
- Admin can override any record — change is audit-logged

#### 26.5 Grace Period

Configurable grace period (default: 15 minutes). Students arriving within the window may be marked Late.
Configured system-wide by Admin.

#### 26.6 Percentage Calculation


```
Formula Attended ÷ Total Sessions × 100
```
```
Counted as Attended Present (P) and Late (L)
```
```
Not Counted Absent (A)
```
```
Excused Sessions Excluded from denominator — not penalised
```
```
Warning Threshold Configurable (default 75%) — visual warning shown to student below
threshold
```
#### 26.7 Database Schema

```
Table Key Fields
```
```
AttendanceSessions Id, TimetableSessionId, LecturerId, Date, TakenAt, IsClosed
```
```
AttendanceRecords Id, AttendanceSessionId, StudentId, Status, Notes, RecordedAt,
OverriddenBy
```
#### 26.8 Reporting

- Lecturer: per-module report with all students and percentages
- Admin: institution-wide report filterable by Faculty, Program, Batch, Module, Date range
- Export to CSV and PDF


## 27. Grade Calculation & Assessment Structure

**Priority: Critical — Without this, the grading module has no defined logic**

#### 27.1 Purpose

Defines how grades are structured, weighted, calculated, and aggregated into final module results including
GPA/CGPA.

#### 27.2 Assessment Component Model

```
Field Description
```
```
ComponentId Unique identifier
```
```
ModuleId Module this component belongs to
```
```
Name e.g. Coursework 1, Mid-Term Exam, Final Exam
```
```
Type Assignment / Exam / Quiz / Presentation / Lab
```
```
Weight (%) Percentage contribution to final grade — all components must total 100%
```
```
MaxMark Maximum achievable raw mark (e.g. 100)
```
```
PassMark Minimum mark to pass this component (optional)
```
#### 27.3 Calculation Logic

```
Component Percentage Raw mark ÷ MaxMark × 100
```
```
Weighted Score Component percentage × Weight ÷ 100
```
```
Final Module Mark Sum of all weighted scores
```
```
Final Grade Determined by mapping final mark to grading scale
```
#### 27.4 Default Grading Scale

```
Grade Mark Range Grade Points Description
```
```
A+ 90 – 100 4.0 Distinction
```
```
A 85 – 89 4.0 Excellent
```
```
A- 80 – 84 3.7 Very Good
```
```
B+ 75 – 79 3.3 Good
```
```
B 70 – 74 3.0 Above Average
```
```
B- 65 – 69 2.7 Average
```
```
C+ 60 – 64 2.3 Satisfactory
```
```
C 55 – 59 2.0 Pass
```

```
D 50 – 54 1.0 Borderline Pass
```
```
F 0 – 49 0.0 Fail
```
```
Admin can modify grade boundaries per institution. Changes are versioned and audit-logged.
```
#### 27.5 GPA & CGPA

```
GPA (Semester) Weighted average of grade points for all modules in the semester,
weighted by credit hours
```
```
CGPA Cumulative GPA across all completed semesters
```
```
Credit Hours Defined per module in course setup
```
```
Incomplete Module Excluded from GPA calculation until all component grades are submitted
```
#### 27.6 Grade Visibility & Appeals

- Grades hidden from students until lecturer publishes them
- Lecturer can publish individual components or all at once
- Published grades trigger a student notification
- Students can submit grade appeals within a configurable window (default: 7 days)
- Appeal includes text reason and optional supporting file
- All appeal actions are audit-logged


## 28. Enrollment Workflow

**Priority: Critical — Underpins every module that references enrolled students**

#### 28.1 Enrollment Modes

```
Mode Description Controlled By
```
```
Admin Enrollment Admin manually enrolls students
individually or in bulk
```
```
Admin
```
```
Batch Auto-Enrollment All students in a Batch auto-enrolled in
their Program modules for the semester
```
```
System (triggered by Admin)
```
```
Lecturer Invite Lecturer generates enrollment code;
students self-enroll using code
```
```
Lecturer
```
```
Self-Enrollment Students request enrollment;
Lecturer/Admin approves
```
```
Student initiates; Admin/Lecturer
approves
```
#### 28.2 Enrollment Rules

- A student cannot be enrolled in the same course twice in the same semester
- Enrollment cap per course — system rejects once full
- Enrollment closes after a configurable deadline
- Dropping enrollment requires Admin approval
- Dropped enrollments are soft-deleted — history retained for academic records

#### 28.3 Enrollment Periods

```
Open Date First date enrollment is permitted
```
```
Close Date Date after which new enrollment is blocked
```
```
Late Override Admin can override close date on a per-student basis
```
#### 28.4 Database Schema

```
Table Key Fields
```
```
Enrollments Id, StudentId, CourseId, SemesterId, EnrolledAt, EnrolledBy, Status
(Active/Dropped/Completed), DroppedAt, DroppedBy
```
```
EnrollmentRequests Id, StudentId, CourseId, RequestedAt, Status
(Pending/Approved/Rejected), ReviewedBy, ReviewedAt
```
#### 28.5 Bulk Enrollment

- Admin uploads CSV with StudentId and CourseId columns
- System validates each row — duplicates and invalid IDs reported without stopping the batch


- Summary report shows successful and failed enrollments


## 29. Password Reset & Account Recovery

**Priority: Significant — Required for basic usability and self-service support**

#### 29.1 Self-Service Reset Flow

- User clicks "Forgot Password" on login page
- User enters registered email address
- System generates a secure single-use reset token (UUID + HMAC signed)
- Token stored with 30-minute expiry
- Reset link emailed: /auth/reset-password?token={token}
- User enters and confirms new password
- Token validated — expired or used tokens prompt user to restart
- Password updated; all active sessions for that user are invalidated
- Confirmation email sent

#### 29.2 Security Rules

```
Token Expiry 30 minutes from generation
```
```
Token Reuse Single-use — invalidated immediately on use
```
```
Rate Limit Maximum 3 reset requests per email per hour
```
```
Response Wording Always "If this email exists, a link has been sent" — never reveals
registration status
```
```
Session Invalidation All refresh tokens revoked on successful reset
```
```
Audit Log Reset events logged with timestamp and IP address
```
#### 29.3 Admin-Initiated Reset

- Admin can trigger forced reset for any user from User Management panel
- User receives reset email and must set new password on next login
- Admin cannot view or set the password directly — trigger only


## 30. JWT Refresh Token Strategy

**Priority: Significant — Without this, users are forced to re-login frequently**

#### 30.1 Token Architecture

```
Token Type Lifespan Storage Purpose
```
```
Access Token (JWT) 15 minutes Angular in-memory service Authenticate API requests
```
```
Refresh Token 7 days HttpOnly secure cookie Obtain new access tokens
silently
```
#### 30.2 Refresh Flow

- API request returns 401 — Angular interceptor automatically calls POST /api/v1/auth/refresh
- Refresh token sent via HttpOnly cookie (never in request body)
- Server validates signature, expiry, and revocation status
- New access token issued; refresh token rotated (old one invalidated)
- Original request retried with new access token
- If refresh fails — user redirected to login

#### 30.3 Revocation Rules

```
Logout Refresh token immediately revoked
```
```
Password Reset All refresh tokens for user revoked
```
```
Admin Deactivation All refresh tokens for user revoked
```
```
Token Reuse Detection Revoked token re-presented → all user tokens revoked; security alert
logged
```
#### 30.4 Database Schema

```
Table Key Fields
```
```
RefreshTokens Id, UserId, TokenHash (SHA-256), IssuedAt, ExpiresAt, RevokedAt,
ReplacedByTokenId, DeviceInfo
```

## 31. Search & Filtering System

**Priority: Significant — Basic usability requirement for all roles**

#### 31.1 Search Scope by Role

```
Role Searchable Entities
```
```
Student Enrolled courses, course materials, announcements, assignments
```
```
Lecturer Their courses, enrolled students, submissions, materials
```
```
Admin All users, all courses, all files, notifications, audit logs
```
#### 31.2 Search API

```
Endpoint GET /api/v1/search
```
```
Parameters q (required, min 2 chars), type (user/course/material/assignment), page,
pageSize
```
```
Response Array of {id, type, title, excerpt, url}
```
```
Max Results 50 per page
```
```
RBAC Results automatically scoped to caller's role — no cross-role data leakage
```
#### 31.3 Module-Level Filtering

- User list: filter by role, status, batch, program, faculty
- Course list: filter by semester, status, lecturer
- Submission list: filter by status (submitted / graded / late / missing)
- Audit log: filter by entity type, action, user, date range

#### 31.4 Implementation Notes

- Use PostgreSQL full-text search (tsvector / tsquery)
- Index all searchable text columns
- Frontend debounces search input at 300ms to reduce API calls


## 32. Audit Log Module

**Priority: Significant — Scattered references throughout docs but never formally defined**

#### 32.1 Events Audited

```
Category Events Logged
```
```
Authentication Login, logout, failed login, password reset
```
```
User Management User created, role changed, deactivated, bulk import
```
```
Enrollment Student enrolled, dropped, bulk enrollment
```
```
Grades Grade entered, updated, published, appeal submitted/resolved
```
```
Files File uploaded, downloaded, deleted
```
```
Timetable Session created, edited, deleted, timetable published
```
```
Attendance Session opened, records submitted, record overridden
```
```
Notifications Notification sent, delivery failed
```
```
System Settings changed, backup triggered, integration toggled
```
#### 32.2 Database Schema

```
Field Type Description
```
```
Id UUID Unique log entry identifier
```
```
Timestamp DateTime (UTC) When the action occurred
```
```
UserId FK (nullable) Who performed the action — null for system events
```
```
UserRole string Role at time of action
```
```
Action string Machine-readable code (e.g. GRADE_UPDATED)
```
```
EntityType string Entity affected (e.g. Submission, User)
```
```
EntityId string ID of the affected record
```
```
Before JSONB State before change (update events)
```
```
After JSONB State after change (create/update events)
```
```
IpAddress string Source IP of the request
```
```
UserAgent string Browser / client information
```
#### 32.3 Retention Policy

```
Online Retention 12 months — queryable via Admin UI
```
```
Archive Retention 5 years — exported to cold storage
```

```
Deletion Policy Audit logs must never be deleted — archived only
```
#### 32.4 Admin UI

- Read-only filterable table: action type, user, entity type, date range
- Export to CSV
- No editing of log entries from UI


## 33. File Download Tracking

**Priority: Significant — Compliance and engagement analytics requirement**

#### 33.1 Purpose

Records every file download event to support compliance, give lecturers material engagement insight, and
provide admin with storage analytics.

#### 33.2 Tracked Data

```
Field Description
```
```
FileId Reference to Files table
```
```
DownloadedBy UserId of the downloader
```
```
DownloadedAt Timestamp (UTC)
```
```
IpAddress Source IP
```
```
CourseContext Course the file was accessed from (if applicable)
```
#### 33.3 Lecturer View

- Per-material download count displayed next to each uploaded file
- Breakdown of which students downloaded a material and when
- Available for course materials — not for submission files

#### 33.4 Privacy Note

Download data visible only to Lecturers (their courses) and Admins. Students cannot see who else
downloaded a file. Governed by institution's data retention policy.


## 34. Semester & Academic Calendar Module

**Priority: Minor — Referenced throughout the system but never defined**

#### 34.1 Academic Year & Semester Model

```
Academic Year Container for all semesters — e.g. 2024/2025
```
```
Semester e.g. Semester 1 (Aug–Dec), Semester 2 (Jan–May), Short Semester
```
```
Start / End Date First and last day of teaching
```
```
Enrollment Open / Close Window during which students can be enrolled
```
```
Grade Submission
Deadline
```
```
Date by which all grades must be published
```
```
Status Upcoming / Active / Completed
```
#### 34.2 Calendar Events

```
Field Description
```
```
Title Event name (e.g. Eid al-Adha Holiday)
```
```
Date / Range Single day or date range
```
```
Type Holiday / Exam Period / Registration / Other
```
```
Audience All / Students Only / Staff Only
```
```
Visibility Student dashboard, Lecturer dashboard
```
#### 34.3 System Behaviour

- Active semester — courses become accessible to enrolled students
- Completed semester — courses become read-only for students
- Assignment late submission logic references semester dates
- Timetable sessions are scoped to a specific semester


## 35. Assignment Rubrics & Marking Criteria

**Priority: Minor — Improves grading consistency and student transparency**

#### 35.1 Purpose

Rubrics allow lecturers to define structured marking criteria, improving grading consistency and giving
students clear expectations before submission.

#### 35.2 Rubric Structure

```
Rubric Set of criteria attached to an assignment
```
```
Criterion Single aspect evaluated (e.g. Code Quality, Analysis Depth)
```
```
Performance Level Descriptive band (e.g. Excellent / Good / Needs Work)
```
```
Points Marks awarded per level per criterion
```
#### 35.3 Workflow

- Lecturer creates rubric while configuring an assignment
- Rubrics saved as reusable templates
- Grading: lecturer selects a performance level per criterion
- System calculates total marks from selected levels
- Student sees rubric feedback alongside grade (when grades published)

#### 35.4 Database Schema

```
Table Key Fields
```
```
Rubrics Id, AssignmentId, Title, IsTemplate, CreatedBy
```
```
RubricCriteria Id, RubricId, Title, MaxPoints, OrderIndex
```
```
RubricLevels Id, CriterionId, Title, Description, Points
```
```
RubricSubmissionScores Id, SubmissionId, CriterionId, SelectedLevelId, LecturerNotes
```

## 36. Student Course Progress Tracking

**Priority: Minor — Improves student engagement and lecturer insight**

#### 36.1 Tracked Activities

```
Activity Event Recorded
```
```
Material Viewed Student opens or downloads a course material
```
```
Assignment Submitted Student submits an assignment
```
```
Grade Received Lecturer publishes a grade for the student
```
```
Announcement Read Student opens an announcement
```
#### 36.2 Progress Indicators

- Student dashboard: progress bar per enrolled course
- Progress = completed activities ÷ total activities × 100
- Submissions weighted higher than material views
- Lecturer dashboard: class-average progress per course

#### 36.3 Future: Prerequisite Gating

Future enhancement — preventing access to later content until earlier materials are completed. Not required
for MVP but the progress tracking data model must support it from day one.


## 37. Rate Limiting & Abuse Protection

**Priority: Minor — Security baseline missing from original documentation**

#### 37.1 Rate Limits by Endpoint

```
Endpoint Group Limit Window Action on Exceed
```
```
POST /auth/login 10 attempts 15 min per IP 429 + 15-min IP block
```
```
POST /auth/forgot-
password
```
```
3 requests 1 hour per
email
```
```
429 — silent
```
```
POST /auth/refresh 60 requests 1 hour per user 429 + force logout
```
```
File upload endpoints 20 uploads 1 hour per user 429
```
```
General API
(authenticated)
```
```
300 requests 1 min per user 429
```
```
General API
(unauthenticated)
```
```
30 requests 1 min per IP 429
```
#### 37.2 Implementation

```
Library AspNetCoreRateLimit (NuGet)
```
```
Storage In-memory (MVP); Redis (production)
```
```
Response Headers X-RateLimit-Limit, X-RateLimit-Remaining, Retry-After
```
```
Admin Exemption Admin role exempt from general limits; login limits still apply
```
#### 37.3 Account Lockout

- 5 failed login attempts for a specific account → account locked for 30 minutes
- Admin can manually unlock from User Management panel
- All lockout events written to Audit Log


## 38. Backup & Recovery Plan

**Priority: Minor — Original documentation mentions automatic backups with no detail**

#### 38.1 Recovery Objectives

```
RTO (Recovery Time Objective) Maximum acceptable service restoration time: 4 hours
```
```
RPO (Recovery Point Objective) Maximum acceptable data loss: 24 hours (standard) or 1 hour (with
WAL archiving)
```
#### 38.2 Backup Schedule

```
Type Frequency Retention Method
```
```
Full Database Backup Daily at 02:00
UTC
```
```
30 days pg_dump — compressed archive
```
```
File Storage Backup Daily at 03:00
UTC
```
```
30 days rsync to secondary storage
```
```
Transaction Log (WAL) Continuous 7 days PostgreSQL WAL archiving
```
```
Configuration Backup On each config
change
```
```
90 days Version-controlled export
```
#### 38.3 Storage & Security

- Primary: cloud object storage (AWS S3 or equivalent)
- Secondary: separate geographic region for disaster recovery
- All backups encrypted at rest — AES- 256
- Access restricted to DevOps role only

#### 38.4 Restoration Procedure

- Admin raises restoration request with target restore point
- DevOps verifies backup integrity via checksum
- System placed into maintenance mode
- Database restored from selected backup
- File storage synced to match database state
- Smoke tests run before maintenance mode lifted
- Restoration event written to Audit Log


## 39. Localization & Multi-Language Support

**Priority: Minor — Infrastructure required from day one; full translation is Phase 2**

#### 39.1 Planned Languages

```
Language Code Direction Status
```
```
English en LTR Primary — required for MVP
```
```
Dhivehi dv RTL Phase 2 — post-MVP
```
```
Arabic ar RTL Optional future addition
```
#### 39.2 Frontend (Angular)

- Use Angular i18n (@angular/localize) or ngx-translate
- All UI strings in translation JSON files — no hardcoded display text
- Date, time, and number formatting via Angular locale-aware pipes
- RTL layout via CSS logical properties (start/end not left/right)

#### 39.3 Backend

- API error messages localizable via Accept-Language header
- Email templates stored per language

#### 39.4 MVP Requirement

```
Full Dhivehi support is NOT required for MVP. However, the i18n infrastructure — translation file structure,
Angular locale setup, no hardcoded strings — must be in place from day one to avoid a costly refactor later.
```


