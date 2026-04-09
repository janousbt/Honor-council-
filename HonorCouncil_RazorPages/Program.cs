using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Options;
using HonorCouncil_RazorPages.Services;
using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SeedUserOptions>(builder.Configuration.GetSection(SeedUserOptions.SectionName));
builder.Services.Configure<NotificationRecipientOptions>(builder.Configuration.GetSection(NotificationRecipientOptions.SectionName));

builder.Services.AddDbContext<HonorCouncilDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HonorCouncilDatabase")));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HonorCouncilStaff", policy =>
        policy.RequireRole("Admin", "President", "Coordinator", "Investigator"));

    options.AddPolicy("FacultyOnly", policy => policy.RequireRole("Faculty"));
    options.AddPolicy("ReportAccess", policy => policy.RequireRole("Faculty", "Admin", "President", "Coordinator", "Investigator"));
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "HonorCouncilStaff");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
    options.Conventions.AllowAnonymousToFolder("/Reports");
    options.Conventions.AuthorizeFolder("/Student", "StudentOnly");
    options.Conventions.AuthorizeFolder("/Faculty", "FacultyOnly");
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IApplicationAuthenticationService, ApplicationAuthenticationService>();
builder.Services.AddScoped<IReportIntakeService, ReportIntakeService>();
builder.Services.AddScoped<IReportSubmissionService, ReportSubmissionService>();
builder.Services.AddScoped<IAdminCaseService, AdminCaseService>();
builder.Services.AddScoped<IFacultyCaseService, FacultyCaseService>();
builder.Services.AddScoped<IStudentCaseService, StudentCaseService>();
builder.Services.AddScoped<IAcademicCalendarService, AcademicCalendarService>();
builder.Services.AddScoped<ICaseWorkflowService, CaseWorkflowService>();
builder.Services.AddScoped<IEvidenceService, EvidenceService>();
builder.Services.AddScoped<ICaseEvidenceService, CaseEvidenceService>();
builder.Services.AddScoped<IHearingService, HearingService>();
builder.Services.AddScoped<IAppealService, AppealService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStudentScheduleService, StudentScheduleService>();
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();
