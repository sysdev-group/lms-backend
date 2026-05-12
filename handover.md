# LMS Backend — Task Handover

## What was done (Task 1: AssignmentService)

### New files created
| File | Purpose |
|------|---------|
| `src/LMS.Application/Interfaces/ICurrentUserService.cs` | Interface: `UserId` + `Role` from HTTP context |
| `src/LMS.Infrastructure/Services/CurrentUserService.cs` | Implementation via `IHttpContextAccessor` |
| `src/LMS.Infrastructure/Services/AssignmentService.cs` | **Main deliverable** — full IAssignmentService impl |

### Existing files modified
| File | Change |
|------|--------|
| `src/LMS.Domain/Entities/Assignment.cs` | Added `AllowLateSubmission` bool property |
| `src/LMS.Application/DTOs/Assignments/AssignmentDtos.cs` | Added `IsDeadlinePassed` + `AllowLateSubmission` to DTO, `CreateAssignmentRequest`, `UpdateAssignmentRequest` |
| `src/LMS.Infrastructure/Services/StubServices.cs` | Removed `AssignmentService` stub class (would conflict) |
| `src/LMS.API/Extensions/ServiceCollectionExtensions.cs` | Added `AddHttpContextAccessor()` + `ICurrentUserService` → `CurrentUserService` scoped registration |

---

## AssignmentService — key decisions

### Role scoping (`EnsureCourseAccessAsync`)
- **Admin**: unrestricted
- **Lecturer**: must be `Course.LecturerId` — throws `UnauthorizedAccessException` otherwise
- **Student**: must have an `Active` enrollment in the course — throws `UnauthorizedAccessException` otherwise

### Deadline validation
- `CreateAsync`: rejects deadline ≤ `DateTime.UtcNow` with `ArgumentException`
- `UpdateAsync`: rejects deadline change if any submission already exists

### AllowLateSubmission
- New entity field (bool, default `false`). Stored and surfaced through the DTO.
- Submission enforcement (checking this flag at submit time) belongs in `SubmissionService`, not here.

### IsDeadlinePassed
- Computed at map time: `DateTime.UtcNow > a.Deadline`. No DB column.

---

## Pending: EF Core migration required

`AllowLateSubmission` was added to the `Assignment` entity. A migration must be generated before the app can run:

```
dotnet ef migrations add AddAssignmentAllowLateSubmission \
  --project src/LMS.Infrastructure \
  --startup-project src/LMS.API
```

Then `dotnet ef database update` (or let the dev auto-migrate in Program.cs).

---

## Next services to implement

The pattern is identical for each: inject `AppDbContext` + `ICurrentUserService`, implement the interface,
map entities → DTOs, throw `KeyNotFoundException` / `UnauthorizedAccessException` / `ArgumentException`.

Priority order (business dependency):

1. **`EnrollmentService`** — `SubmissionService` and `AssignmentService.EnsureCourseAccess` depend on
   the `Enrollments` table being correctly populated. Implement: enroll, drop (soft-delete), get by student,
   get by course. See docs Section 28.

2. **`SubmissionService`** — depends on Enrollment and Assignment being ready. Key logic:
   - Check `Assignment.Deadline` and `AllowLateSubmission` flag at submit time
   - Set `Submission.IsLate` accordingly
   - Enforce `AllowResubmission` rule (reject second submit if false)
   - See docs Section 7.5.

3. **`GradeService`** — depends on Submission. Implement grade visibility (published vs unpublished),
   bulk publish, GPA calculation. See docs Section 7.6 and Section 27.

4. **`CourseService`** — relatively self-contained. Students see enrolled courses only; lecturers see their
   own. Pagination via `PaginatedResult<T>`. See `IServices.cs` → `ICourseService`.

5. **`NotificationService`** — in-app notifications. Straightforward CRUD + mark-read. See Section 23.

---

## Patterns to follow

- Constructor: `AppDbContext _db` + `ICurrentUserService _currentUser`
- Always `async/await` — never `.Result` / `.Wait()`
- Return DTOs — map via a private `static MapToDto(Entity e)` helper
- Throw `KeyNotFoundException` for missing records, `UnauthorizedAccessException` for RBAC failures,
  `ArgumentException` for business rule violations
- `ExceptionHandlingMiddleware` maps all three automatically — no try/catch needed in services
- Methods max ~30 lines — extract private helpers (`EnsureCourseAccessAsync` is the established pattern)

## Architecture reference
- `src/LMS.Infrastructure/Services/AuthService.cs` — the worked example (read this first)
- `src/LMS.Application/Interfaces/IServices.cs` — all interface contracts
- `src/LMS.Infrastructure/Services/StubServices.cs` — remaining stubs to replace
- `src/LMS.API/Extensions/ServiceCollectionExtensions.cs` — where to register new services
