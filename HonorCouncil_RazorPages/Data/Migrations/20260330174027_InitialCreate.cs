using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HonorCouncil_RazorPages.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Term = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacultyMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    RoleTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacultyMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Investigators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investigators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Major = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AcademicYear = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseFacultyAssignments",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    FacultyMemberId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseFacultyAssignments", x => new { x.CourseId, x.FacultyMemberId });
                    table.ForeignKey(
                        name: "FK_CourseFacultyAssignments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseFacultyAssignments_FacultyMembers_FacultyMemberId",
                        column: x => x.FacultyMemberId,
                        principalTable: "FacultyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    SubmittedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViolationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViolationDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsWithinFormalFilingWindow = table.Column<bool>(type: "bit", nullable: false),
                    WasRedirectedToFormal = table.Column<bool>(type: "bit", nullable: false),
                    SubmissionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FacultyMemberId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_FacultyMembers_FacultyMemberId",
                        column: x => x.FacultyMemberId,
                        principalTable: "FacultyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HonorCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    CurrentStatus = table.Column<int>(type: "int", nullable: false),
                    PriorityLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssignedInvestigatorId = table.Column<int>(type: "int", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HonorCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HonorCases_Investigators_AssignedInvestigatorId",
                        column: x => x.AssignedInvestigatorId,
                        principalTable: "Investigators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HonorCases_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appeals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HonorCaseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Grounds = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appeals_HonorCases_HonorCaseId",
                        column: x => x.HonorCaseId,
                        principalTable: "HonorCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvailabilitySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HonorCaseId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: true),
                    FacultyMemberId = table.Column<int>(type: "int", nullable: true),
                    ParticipantRole = table.Column<int>(type: "int", nullable: false),
                    SubmittedByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubmittedByEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilitySlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailabilitySlots_FacultyMembers_FacultyMemberId",
                        column: x => x.FacultyMemberId,
                        principalTable: "FacultyMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AvailabilitySlots_HonorCases_HonorCaseId",
                        column: x => x.HonorCaseId,
                        principalTable: "HonorCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvailabilitySlots_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CaseStatusEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HonorCaseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OccurredUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedByDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseStatusEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseStatusEntries_HonorCases_HonorCaseId",
                        column: x => x.HonorCaseId,
                        principalTable: "HonorCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvidenceFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HonorCaseId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UploadedByRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenceFiles_HonorCases_HonorCaseId",
                        column: x => x.HonorCaseId,
                        principalTable: "HonorCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hearings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HonorCaseId = table.Column<int>(type: "int", nullable: false),
                    ScheduledStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HearingFormat = table.Column<int>(type: "int", nullable: false),
                    LocationOrMeetingLink = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hearings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hearings_HonorCases_HonorCaseId",
                        column: x => x.HonorCaseId,
                        principalTable: "HonorCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HonorCaseId = table.Column<int>(type: "int", nullable: true),
                    ReportId = table.Column<int>(type: "int", nullable: true),
                    RecipientEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SentUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationLogs_HonorCases_HonorCaseId",
                        column: x => x.HonorCaseId,
                        principalTable: "HonorCases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationLogs_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Witnesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HonorCaseId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    Affiliation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastNotifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Witnesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Witnesses_HonorCases_HonorCaseId",
                        column: x => x.HonorCaseId,
                        principalTable: "HonorCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Investigators",
                columns: new[] { "Id", "Email", "FullName", "IsActive" },
                values: new object[,]
                {
                    { 1, "aharper@jmu.edu", "Alex Harper", true },
                    { 2, "mlee@jmu.edu", "Morgan Lee", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appeals_HonorCaseId",
                table: "Appeals",
                column: "HonorCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_FacultyMemberId",
                table: "AvailabilitySlots",
                column: "FacultyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_HonorCaseId",
                table: "AvailabilitySlots",
                column: "HonorCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_StudentId",
                table: "AvailabilitySlots",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStatusEntries_HonorCaseId",
                table: "CaseStatusEntries",
                column: "HonorCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseFacultyAssignments_FacultyMemberId",
                table: "CourseFacultyAssignments",
                column: "FacultyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceFiles_HonorCaseId",
                table: "EvidenceFiles",
                column: "HonorCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FacultyMembers_Email",
                table: "FacultyMembers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hearings_HonorCaseId",
                table: "Hearings",
                column: "HonorCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HonorCases_AssignedInvestigatorId",
                table: "HonorCases",
                column: "AssignedInvestigatorId");

            migrationBuilder.CreateIndex(
                name: "IX_HonorCases_CaseNumber",
                table: "HonorCases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HonorCases_ReportId",
                table: "HonorCases",
                column: "ReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_HonorCaseId",
                table: "NotificationLogs",
                column: "HonorCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_ReportId",
                table: "NotificationLogs",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CourseId",
                table: "Reports",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_FacultyMemberId",
                table: "Reports",
                column: "FacultyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportNumber",
                table: "Reports",
                column: "ReportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StudentId",
                table: "Reports",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumber",
                table: "Students",
                column: "StudentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Witnesses_HonorCaseId",
                table: "Witnesses",
                column: "HonorCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appeals");

            migrationBuilder.DropTable(
                name: "AvailabilitySlots");

            migrationBuilder.DropTable(
                name: "CaseStatusEntries");

            migrationBuilder.DropTable(
                name: "CourseFacultyAssignments");

            migrationBuilder.DropTable(
                name: "EvidenceFiles");

            migrationBuilder.DropTable(
                name: "Hearings");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "Witnesses");

            migrationBuilder.DropTable(
                name: "HonorCases");

            migrationBuilder.DropTable(
                name: "Investigators");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "FacultyMembers");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
