# LMS Project — Handover Document

## Project Overview

Two repos side by side:
```
your-folder/
  lms-backend/    ← ASP.NET Core 8 Web API
  lms-frontend/   ← Angular frontend
```

**Backend:** Clean Architecture (Domain → Application → Infrastructure → API), EF Core 8, PostgreSQL, BCrypt.Net, JWT Bearer.  
**Frontend:** Angular (standalone components, signals for state, reactive forms, `ApiService` wrapping `HttpClient`).  
**Run everything:** `docker-compose up` from `lms-backend/` — starts API (:5001), Swagger (:5001/swagger), DB (:5432), frontend (:4200).

---

## Task 1 — UserService (Backend) ✅ DONE

**Branch:** `feature/aiman/user-service` (pushed)  
**Commit:** `feat: implement UserService — CRUD, deactivation, password reset, bulk CSV import`

### Files changed
| File | Change |
|------|--------|
| `src/LMS.Infrastructure/Services/UserService.cs` | New — full implementation |
| `src/LMS.Application/Interfaces/IServices.cs` | Added `BulkImportAsync(Stream)` to `IUserService` |
| `src/LMS.Application/DTOs/Users/UserDtos.cs` | Added `BulkImportResult` DTO |
| `src/LMS.Infrastructure/Services/StubServices.cs` | Removed `UserService` stub |
| `src/LMS.Infrastructure/LMS.Infrastructure.csproj` | Added JWT Bearer package (pre-existing build error fix) |
| `src/LMS.API/Controllers/StubControllers.cs` | Added missing `using` for Submissions namespace (pre-existing build error fix) |

### Known issues identified in review (not fixed yet)
1. **Race condition on email uniqueness** — wrap `SaveChangesAsync` in try/catch for `DbUpdateException` in `CreateUserAsync` and `UpdateUserAsync`
2. **`StreamReader` closes caller's stream** — change to `new StreamReader(csvStream, leaveOpen: true)` in `BulkImportAsync`
3. **No email format validation in bulk import** — add per-row email format check
4. **No row limit on bulk import** — add a cap (e.g. 1,000 rows)
5. **Search uses `.ToLower().Contains()`** — prefer `EF.Functions.ILike()` for PostgreSQL

---

## Task 2 — FileService (Backend) ❌ TODO

**Spec:** LMS Full Documentation v2, Section 21 — File Upload & Storage Architecture  
**File to create:** `lms-backend/src/LMS.Infrastructure/Services/FileService.cs`  
**Stub to remove from:** `lms-backend/src/LMS.Infrastructure/Services/StubServices.cs`  
**Interface already exists:** `IFileService` in `src/LMS.Application/Interfaces/IServices.cs`

### Requirements
- Upload, download, and delete operations
- **File type whitelist:** PDF, DOCX, PPTX, JPG, PNG, ZIP, .py, .java, .cs, .js, .ts, MP4
- **Max size:** 25 MB — validate server-side
- Sanitise filenames — strip path traversal characters (e.g. `../`, `..\\`)
- Store files under `/uploads/{courses|assignments|submissions|profiles}/`
- Save metadata to `Files` table: `FileName`, `OriginalName`, `Path`, `Size`, `UploadedBy`, `UploadDate`, `RelatedEntity`, `EntityId`
- Files must NOT be directly URL-accessible — serve only through authenticated download endpoints
- Follow `UserService.cs` pattern: inject `AppDbContext`, async/await, typed exceptions, XML docs

### Interface signature (already defined)
```csharp
Task<Guid> UploadAsync(Stream fileStream, string originalName, string mimeType,
    string relatedEntity, Guid entityId, Guid uploadedById);
Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(Guid fileId, Guid requestingUserId);
Task DeleteAsync(Guid fileId, Guid requestingUserId);
```

---

## Task 3 — User List UI (Frontend) ❌ TODO

**Spec:** Section 7.3  
**File:** `lms-frontend/src/app/features/users/user-list/user-list.component.ts` (+ template + styles)

### Requirements
- Paginated list of users
- Filters: role, status, batch, program, faculty (Section 31.3)
- Columns per row: name, email, role, status
- Row actions: edit (navigate to detail), deactivate
- Use `UserService` from `core/services/user.service.ts`
- Match the project's existing UI framework (Angular Material or Tailwind — check first)
- Mobile responsive

---

## Task 4 — User Detail/Edit UI (Frontend) ❌ TODO

**Spec:** Section 7.3  
**File:** `lms-frontend/src/app/features/users/user-detail/user-detail.component.ts` (+ template + styles)

### Requirements
- Display full profile: name, email, role, status, batch/program if applicable
- Editable fields: name, role, status — reactive form with validation
- Admin can trigger password reset (calls `POST /api/v1/auth/forgot-password` with the user's email — admin does NOT set the password directly, per Section 29.3)
- Confirmation dialog before deactivating a user
- Use `UserService` from `core/services/user.service.ts`

---

## Task 5 — File Upload Component (Frontend) ❌ TODO

**Spec:** Section 21  
**File:** `lms-frontend/src/app/shared/components/file-upload/file-upload.component.ts` (+ template + styles)

### Requirements
- Reusable — accepts allowed file types and max size as `@Input()` props
- Client-side validation before upload: type whitelist + 25 MB limit
- Clear error messages for rejected files
- Upload progress indicator
- On success, emit returned file metadata to parent via `@Output()`
- Use `FileService` from `core/services/file.service.ts`
- Mobile-friendly: touch-friendly drag or tap-to-select

---

## Task 6 — Angular UserService ❌ TODO

**Spec:** Sections 7.3 and 25.5  
**File:** `lms-frontend/src/app/core/services/user.service.ts`

### Requirements
- Methods: `getUsers(filters)`, `getUserById(id)`, `createUser(data)`, `updateUser(id, data)`, `deactivateUser(id)`, `bulkImport(csvFile)`
- All return `Observable`s
- Base URL: `/api/v1/users`
- Auth via existing HTTP interceptor — do NOT handle auth here
- Handle the standard API envelope (`{ success, data, message, errors }` — Section 25.3)
- Use `HttpClient` — no `fetch()`

---

## Task 7 — Angular FileService ❌ TODO

**Spec:** Sections 21 and 25.5  
**File:** `lms-frontend/src/app/core/services/file.service.ts`

### Requirements
- Methods: `uploadFile(file, relatedEntity, entityId)`, `downloadFile(fileId)`, `deleteFile(fileId)`
- `uploadFile` must use `reportProgress: true` so progress can be tracked
- `downloadFile` triggers a browser file download using the response blob
- All return `Observable`s
- Base URL: `/api/v1/files`
- Auth via existing HTTP interceptor
- Handle standard API envelope (Section 25.3)

---

## Task 8 — UI Polish Pass (Frontend) ❌ TODO

Review all implemented frontend pages for visual and UX consistency:

- Spacing, font sizes, and colours consistent across pages
- Button styles consistent (size, colour, shape)
- Loading states on all data fetches (spinner or skeleton)
- Empty states handled — no blank white boxes
- Error states shown to user (not just console)
- No orphaned `console.log` statements
- Page titles and labels correct and consistently capitalised

---

## Task 9 — Mobile Responsiveness Check (Frontend) ❌ TODO

Test all pages at **375px viewport width** (iPhone SE) and fix any issues:

- No horizontal scroll
- Tables collapse or scroll horizontally
- Forms stack vertically on small screens
- Buttons/tap targets at least 44×44px
- Navigation/sidebar collapses correctly
- File upload works with tap-to-select
- Text readable — no overflow or clipping

---

## Backend architecture rules (from CONTRIBUTING.md)

- Reference implementations: `AuthService.cs` + `AuthController.cs` (worked examples) and `UserService.cs` (second example)
- Inject `AppDbContext` via constructor — never instantiate directly
- Always `async`/`await` — never `.Result` or `.Wait()`
- Map to DTOs before returning — never return domain entities
- Methods max ~30 lines — extract private helpers if longer
- Throw typed exceptions — middleware maps them automatically:
  - `KeyNotFoundException` → 404
  - `InvalidOperationException` → 422
  - `UnauthorizedAccessException` → 401
  - `ArgumentException` → 400
- XML doc comments on all public methods
- Branch naming: `feature/your-name/module-name`
- Assign **Iyaadh** as PR reviewer

### Adding a new backend service
1. Find stub class in `src/LMS.Infrastructure/Services/StubServices.cs`
2. Create `src/LMS.Infrastructure/Services/<Name>Service.cs`
3. Remove the stub class from `StubServices.cs`
4. DI is already wired in `src/LMS.API/Extensions/ServiceCollectionExtensions.cs` — no change needed

---

## Frontend architecture rules (from CONTRIBUTING.md)

- All components are **standalone** — add imports explicitly
- Use **signals** for local UI state — not plain class properties
- Use **reactive forms** for forms with validation
- Never inject `HttpClient` directly — always use `ApiService`
- Never call `.subscribe()` in a service — only in components
- Handle errors in `subscribe({ error: ... })` — `errorInterceptor` shows the snackbar automatically

---

## Backend service status

| Service | Section | Status |
|---------|---------|--------|
| `UserService` | 7.3 | ✅ Done |
| `FileService` | 21 | ❌ Task 2 |
| `CourseService` | 7.4 | ❌ Stub |
| `AssignmentService` | 7.5 | ❌ Stub |
| `SubmissionService` | 7.5 | ❌ Stub |
| `GradeService` | 7.6 + 27 | ❌ Stub |
| `NotificationService` | 7.7 + 23 | ❌ Stub |
| `EnrollmentService` | 28 | ❌ Stub |
| `TimetableService` | 24 | ❌ Stub |
| `AttendanceService` | 26 | ❌ Stub |
| `AuditService` | 32 | ❌ Stub |

---

## How to start each new session

1. Open a new conversation
2. Share this file + the relevant section(s) of `LMS_Full_Documentation_v2.md`
3. State the task number and what to implement
4. For backend tasks: Claude should read `UserService.cs` as the pattern reference
5. For frontend tasks: Claude should read the existing Angular service/component files first to match conventions
