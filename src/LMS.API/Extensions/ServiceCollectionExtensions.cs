using System.Text;
using LMS.Application.Interfaces;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Interfaces;
using LMS.Infrastructure.Services;
using LMS.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace LMS.API.Extensions;

/// <summary>
/// Extension methods that keep Program.cs clean by grouping related service registrations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers EF Core with PostgreSQL.</summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        return services;
    }

    /// <summary>
    /// Registers JWT Bearer authentication.
    /// Access tokens expire in 15 minutes. Refresh token rotation handled in AuthService.
    /// See Section 30 — JWT Refresh Token Strategy.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var secret = config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero   // No grace period — token expiry is exact
                };
            });

        return services;
    }

    /// <summary>Registers Swagger with JWT auth support.</summary>
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Modern Modular LMS API",
                Version = "v1",
                Description = "API for the Modern Modular Learning Management System"
            });

            // Include XML comments in Swagger UI
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

            // Add JWT auth button to Swagger UI
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT access token."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Registers all application services with the DI container.
    /// When you implement a new service, add it here.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddOptions<CloudinarySettings>().BindConfiguration("Cloudinary");
        services.AddOptions<ResendSettings>().BindConfiguration("Resend");

        services.AddScoped<IEmailService, EmailService>();

        // ── Worked example ────────────────────────────────────────────────────
        services.AddScoped<IAuthService, AuthService>();

        // ── Programme (additive hierarchy layer above courses) ─────────────────
        services.AddScoped<IProgrammeService, ProgrammeService>();

        // ── Stub services — replace NotImplementedException methods with real logic ──
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<ITimetableService, TimetableService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }

    /// <summary>Configures CORS to allow the Angular dev server.</summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AngularDev", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Required for HttpOnly cookie (refresh token)
            });
        });

        return services;
    }
}
