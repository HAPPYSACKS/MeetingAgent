using MeetingAgent.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace MeetingAgent.Infrastructure.Persistence;

public sealed class MeetingAgentDbContext(DbContextOptions<MeetingAgentDbContext> options) : DbContext(options)
{
    public DbSet<MeetingSessionRecord> MeetingSessions => Set<MeetingSessionRecord>();

    public DbSet<AgendaPlanRecord> AgendaPlans => Set<AgendaPlanRecord>();

    public DbSet<AgendaSectionRecord> AgendaSections => Set<AgendaSectionRecord>();

    public DbSet<FacilitatorAlertRecord> FacilitatorAlerts => Set<FacilitatorAlertRecord>();

    public DbSet<MeetingRecapRecord> MeetingRecaps => Set<MeetingRecapRecord>();

    public DbSet<RecapInsightRecord> RecapInsights => Set<RecapInsightRecord>();

    public DbSet<RecapActionItemRecord> RecapActionItems => Set<RecapActionItemRecord>();

    public DbSet<TranscriptArtifactRecord> TranscriptArtifacts => Set<TranscriptArtifactRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeetingSessionRecord>(entity =>
        {
            entity.ToTable("MeetingSessions");
            entity.HasKey(meeting => meeting.Id);
            entity.Property(meeting => meeting.OrganizerIdentity).HasMaxLength(320).IsRequired();
            entity.Property(meeting => meeting.Title).HasMaxLength(512).IsRequired();
            entity.Property(meeting => meeting.TeamsMeetingId).HasMaxLength(512).IsRequired();
            entity.Property(meeting => meeting.TeamsChatId).HasMaxLength(512);
            entity.Property(meeting => meeting.CalendarEventId).HasMaxLength(512);
            entity.HasIndex(meeting => meeting.TeamsMeetingId);
            entity.HasIndex(meeting => meeting.OrganizerIdentity);
            entity.HasIndex(meeting => meeting.ScheduledEnd);
        });

        modelBuilder.Entity<AgendaPlanRecord>(entity =>
        {
            entity.ToTable("AgendaPlans");
            entity.HasKey(plan => plan.Id);
            entity.Property(plan => plan.Objective).HasMaxLength(2048).IsRequired();
            entity.Property(plan => plan.ApprovedBy).HasMaxLength(320);
            entity.HasIndex(plan => new { plan.MeetingId, plan.Version }).IsUnique();
            entity.HasIndex(plan => new { plan.MeetingId, plan.ApprovalState, plan.Version });
            entity.HasOne(plan => plan.Meeting)
                .WithMany(meeting => meeting.AgendaPlans)
                .HasForeignKey(plan => plan.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgendaSectionRecord>(entity =>
        {
            entity.ToTable("AgendaSections");
            entity.HasKey(section => section.Id);
            entity.Property(section => section.Title).HasMaxLength(512).IsRequired();
            entity.Property(section => section.Purpose).HasMaxLength(2048).IsRequired();
            entity.Property(section => section.FacilitationNotes).HasMaxLength(2048);
            entity.HasIndex(section => new { section.AgendaPlanId, section.Order }).IsUnique();
            entity.HasOne(section => section.AgendaPlan)
                .WithMany(plan => plan.AgendaSections)
                .HasForeignKey(section => section.AgendaPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FacilitatorAlertRecord>(entity =>
        {
            entity.ToTable("FacilitatorAlerts");
            entity.HasKey(alert => alert.Id);
            entity.Property(alert => alert.EvidenceSnippet).HasMaxLength(2048).IsRequired();
            entity.Property(alert => alert.Recommendation).HasMaxLength(2048).IsRequired();
            entity.HasIndex(alert => new { alert.MeetingId, alert.Timestamp });
            entity.HasOne(alert => alert.Meeting)
                .WithMany(meeting => meeting.FacilitatorAlerts)
                .HasForeignKey(alert => alert.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MeetingRecapRecord>(entity =>
        {
            entity.ToTable("MeetingRecaps");
            entity.HasKey(recap => recap.Id);
            entity.Property(recap => recap.PacingSummary).HasMaxLength(4096).IsRequired();
            entity.HasIndex(recap => recap.MeetingId);
            entity.HasIndex(recap => recap.GeneratedAt);
            entity.HasOne(recap => recap.Meeting)
                .WithMany(meeting => meeting.MeetingRecaps)
                .HasForeignKey(recap => recap.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecapInsightRecord>(entity =>
        {
            entity.ToTable("RecapInsights");
            entity.HasKey(insight => insight.Id);
            entity.Property(insight => insight.Summary).HasMaxLength(2048).IsRequired();
            entity.Property(insight => insight.EvidenceSnippet).HasMaxLength(2048).IsRequired();
            entity.HasIndex(insight => new { insight.MeetingRecapId, insight.Kind });
            entity.HasOne(insight => insight.MeetingRecap)
                .WithMany(recap => recap.Insights)
                .HasForeignKey(insight => insight.MeetingRecapId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecapActionItemRecord>(entity =>
        {
            entity.ToTable("RecapActionItems");
            entity.HasKey(actionItem => actionItem.Id);
            entity.Property(actionItem => actionItem.Description).HasMaxLength(2048).IsRequired();
            entity.Property(actionItem => actionItem.Owner).HasMaxLength(320);
            entity.HasOne(actionItem => actionItem.MeetingRecap)
                .WithMany(recap => recap.ActionItems)
                .HasForeignKey(actionItem => actionItem.MeetingRecapId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TranscriptArtifactRecord>(entity =>
        {
            entity.ToTable("TranscriptArtifacts");
            entity.HasKey(artifact => artifact.Id);
            entity.Property(artifact => artifact.BlobContainerName).HasMaxLength(128).IsRequired();
            entity.Property(artifact => artifact.BlobName).HasMaxLength(1024).IsRequired();
            entity.HasIndex(artifact => artifact.ExpiresAt);
            entity.HasIndex(artifact => new { artifact.MeetingId, artifact.CreatedAt });
            entity.HasOne(artifact => artifact.Meeting)
                .WithMany(meeting => meeting.TranscriptArtifacts)
                .HasForeignKey(artifact => artifact.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
