using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingAgent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizerIdentity = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ScheduledStart = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ScheduledEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TeamsMeetingId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    TeamsChatId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CalendarEventId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgendaPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeetingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    TotalDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ApprovalState = table.Column<int>(type: "int", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendaPlans_MeetingSessions_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "MeetingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacilitatorAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeetingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlertType = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    EvidenceSnippet = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    IsDismissed = table.Column<bool>(type: "bit", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilitatorAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilitatorAlerts_MeetingSessions_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "MeetingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingRecaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeetingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PacingSummary = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TranscriptAvailability = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingRecaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingRecaps_MeetingSessions_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "MeetingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TranscriptArtifacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeetingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlobContainerName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranscriptArtifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranscriptArtifacts_MeetingSessions_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "MeetingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgendaSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgendaPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    SuggestedDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    FacilitationNotes = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendaSections_AgendaPlans_AgendaPlanId",
                        column: x => x.AgendaPlanId,
                        principalTable: "AgendaPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecapActionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingRecapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecapActionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecapActionItems_MeetingRecaps_MeetingRecapId",
                        column: x => x.MeetingRecapId,
                        principalTable: "MeetingRecaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecapInsights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingRecapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    EvidenceSnippet = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecapInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecapInsights_MeetingRecaps_MeetingRecapId",
                        column: x => x.MeetingRecapId,
                        principalTable: "MeetingRecaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgendaPlans_MeetingId_ApprovalState_Version",
                table: "AgendaPlans",
                columns: new[] { "MeetingId", "ApprovalState", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_AgendaPlans_MeetingId_Version",
                table: "AgendaPlans",
                columns: new[] { "MeetingId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgendaSections_AgendaPlanId_Order",
                table: "AgendaSections",
                columns: new[] { "AgendaPlanId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilitatorAlerts_MeetingId_Timestamp",
                table: "FacilitatorAlerts",
                columns: new[] { "MeetingId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRecaps_GeneratedAt",
                table: "MeetingRecaps",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRecaps_MeetingId",
                table: "MeetingRecaps",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSessions_OrganizerIdentity",
                table: "MeetingSessions",
                column: "OrganizerIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSessions_ScheduledEnd",
                table: "MeetingSessions",
                column: "ScheduledEnd");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSessions_TeamsMeetingId",
                table: "MeetingSessions",
                column: "TeamsMeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_RecapActionItems_MeetingRecapId",
                table: "RecapActionItems",
                column: "MeetingRecapId");

            migrationBuilder.CreateIndex(
                name: "IX_RecapInsights_MeetingRecapId_Kind",
                table: "RecapInsights",
                columns: new[] { "MeetingRecapId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptArtifacts_ExpiresAt",
                table: "TranscriptArtifacts",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptArtifacts_MeetingId_CreatedAt",
                table: "TranscriptArtifacts",
                columns: new[] { "MeetingId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendaSections");

            migrationBuilder.DropTable(
                name: "FacilitatorAlerts");

            migrationBuilder.DropTable(
                name: "RecapActionItems");

            migrationBuilder.DropTable(
                name: "RecapInsights");

            migrationBuilder.DropTable(
                name: "TranscriptArtifacts");

            migrationBuilder.DropTable(
                name: "AgendaPlans");

            migrationBuilder.DropTable(
                name: "MeetingRecaps");

            migrationBuilder.DropTable(
                name: "MeetingSessions");
        }
    }
}
