using HonorCouncil_RazorPages.Models;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Data;

public class HonorCouncilDbContext(DbContextOptions<HonorCouncilDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<FacultyMember> FacultyMembers => Set<FacultyMember>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseFaculty> CourseFacultyAssignments => Set<CourseFaculty>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<HonorCase> HonorCases => Set<HonorCase>();
    public DbSet<EvidenceFile> EvidenceFiles => Set<EvidenceFile>();
    public DbSet<StudentScheduleFile> StudentScheduleFiles => Set<StudentScheduleFile>();
    public DbSet<Witness> Witnesses => Set<Witness>();
    public DbSet<Investigator> Investigators => Set<Investigator>();
    public DbSet<CaseStatusEntry> CaseStatusEntries => Set<CaseStatusEntry>();
    public DbSet<Hearing> Hearings => Set<Hearing>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Appeal> Appeals => Set<Appeal>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CourseFaculty>()
            .HasKey(x => new { x.CourseId, x.FacultyMemberId });

        modelBuilder.Entity<Student>()
            .HasIndex(x => x.StudentNumber)
            .IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<FacultyMember>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Investigator>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Report>()
            .HasIndex(x => x.ReportNumber)
            .IsUnique();

        modelBuilder.Entity<HonorCase>()
            .HasIndex(x => x.CaseNumber)
            .IsUnique();

        modelBuilder.Entity<Report>()
            .HasOne(x => x.HonorCase)
            .WithOne(x => x.Report)
            .HasForeignKey<HonorCase>(x => x.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HonorCase>()
            .HasOne(x => x.AssignedInvestigator)
            .WithMany(x => x.AssignedCases)
            .HasForeignKey(x => x.AssignedInvestigatorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HonorCase>()
            .HasOne(x => x.Hearing)
            .WithOne(x => x.HonorCase)
            .HasForeignKey<Hearing>(x => x.HonorCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HonorCase>()
            .HasOne(x => x.Appeal)
            .WithOne(x => x.HonorCase)
            .HasForeignKey<Appeal>(x => x.HonorCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AvailabilitySlot>()
            .HasOne(x => x.Student)
            .WithMany(x => x.AvailabilitySlots)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AvailabilitySlot>()
            .HasOne(x => x.FacultyMember)
            .WithMany(x => x.AvailabilitySlots)
            .HasForeignKey(x => x.FacultyMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Investigator>().HasData(
            new Investigator { Id = 1, FullName = "Alex Harper", Email = "aharper@jmu.edu", IsActive = true },
            new Investigator { Id = 2, FullName = "Morgan Lee", Email = "mlee@jmu.edu", IsActive = true });
    }
}
