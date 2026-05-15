using LMS.Domain.Entities;
using LMS.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data;

/// <summary>
/// The single EF Core database context for the LMS system.
/// All entity configurations are done here via Fluent API.
/// To add a migration: dotnet ef migrations add MigrationName --project src/LMS.Infrastructure --startup-project src/LMS.API
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ─── DbSets ───────────────────────────────────────────────────────────────
    public DbSet<Programme> Programmes => Set<Programme>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<FileRecord> Files => Set<FileRecord>();
    public DbSet<TimetableSession> TimetableSessions => Set<TimetableSession>();
    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── User ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            e.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            e.Property(u => u.PasswordHash).IsRequired();
        });

        // ─── RefreshToken ─────────────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.TokenHash);
            e.HasOne(t => t.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Course ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Course>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Code).IsRequired().HasMaxLength(20);
            e.Property(c => c.Title).IsRequired().HasMaxLength(200);
            e.HasOne(c => c.Lecturer)
             .WithMany()
             .HasForeignKey(c => c.LecturerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Semester)
             .WithMany(s => s.Courses)
             .HasForeignKey(c => c.SemesterId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Assignment ───────────────────────────────────────────────────────
        modelBuilder.Entity<Assignment>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Course)
             .WithMany(c => c.Assignments)
             .HasForeignKey(a => a.CourseId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.CreatedBy)
             .WithMany()
             .HasForeignKey(a => a.CreatedById)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Submission ───────────────────────────────────────────────────────
        modelBuilder.Entity<Submission>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Assignment)
             .WithMany(a => a.Submissions)
             .HasForeignKey(s => s.AssignmentId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Student)
             .WithMany()
             .HasForeignKey(s => s.StudentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Grade)
             .WithOne(g => g.Submission)
             .HasForeignKey<Grade>(g => g.SubmissionId);
        });

        // ─── Enrollment ───────────────────────────────────────────────────────
        modelBuilder.Entity<Enrollment>(e =>
        {
            e.HasKey(en => en.Id);
            // A student cannot be enrolled in the same course in the same semester twice
            e.HasIndex(en => new { en.StudentId, en.CourseId, en.SemesterId }).IsUnique();
            e.HasOne(en => en.Student)
             .WithMany(u => u.Enrollments)
             .HasForeignKey(en => en.StudentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(en => en.Course)
             .WithMany(c => c.Enrollments)
             .HasForeignKey(en => en.CourseId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Notification ─────────────────────────────────────────────────────
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasOne(n => n.Recipient)
             .WithMany(u => u.Notifications)
             .HasForeignKey(n => n.RecipientId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.Sender)
             .WithMany()
             .HasForeignKey(n => n.SenderId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── TimetableSession ─────────────────────────────────────────────────
        modelBuilder.Entity<TimetableSession>(e =>
        {
            e.HasKey(ts => ts.Id);
            e.HasOne(ts => ts.Course)
             .WithMany()
             .HasForeignKey(ts => ts.CourseId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ts => ts.Lecturer)
             .WithMany()
             .HasForeignKey(ts => ts.LecturerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── AttendanceSession ────────────────────────────────────────────────
        modelBuilder.Entity<AttendanceSession>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.TimetableSession)
             .WithMany(ts => ts.AttendanceSessions)
             .HasForeignKey(s => s.TimetableSessionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── AttendanceRecord ─────────────────────────────────────────────────
        modelBuilder.Entity<AttendanceRecord>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.AttendanceSession)
             .WithMany(s => s.Records)
             .HasForeignKey(r => r.AttendanceSessionId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Student)
             .WithMany()
             .HasForeignKey(r => r.StudentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── AuditLog ─────────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Timestamp);
            e.HasIndex(a => a.EntityType);
            e.HasIndex(a => a.UserId);
            // No FK to User — logs must survive user deletion
        });

        // ─── Programme ────────────────────────────────────────────────────────
        modelBuilder.Entity<Programme>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.Code).IsUnique();
            e.Property(p => p.Code).IsRequired().HasMaxLength(20);
            e.Property(p => p.Title).IsRequired().HasMaxLength(200);
            e.Property(p => p.Description).HasMaxLength(1000);
            e.Property(p => p.Department).HasMaxLength(200);
            // Shadow FK — adds nullable ProgrammeId column to Courses table without touching Domain
            e.HasMany(p => p.Courses)
             .WithOne()
             .HasForeignKey("ProgrammeId")
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
