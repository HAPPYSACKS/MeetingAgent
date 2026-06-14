using System.ComponentModel.DataAnnotations;
using MeetingAgent.Application.Agenda;
using MeetingAgent.Application.Security;
using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MeetingAgent.Web.Pages;

public class SetupModel(
    IAgendaDraftingService agendaDraftingService,
    ICurrentUserContext currentUserContext,
    IMeetingSessionRepository meetingSessionRepository,
    IAgendaPlanRepository agendaPlanRepository,
    IAuditLogger auditLogger) : PageModel
{
    [BindProperty]
    public SetupInput Input { get; set; } = new();

    public bool HasDraft => Input.Sections.Count > 0;

    public bool IsApproved { get; private set; }

    public string? StatusMessage { get; private set; }

    public void OnGet()
    {
        Input.MeetingId = Guid.NewGuid();
    }

    public IActionResult OnPostGenerate()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var draft = agendaDraftingService.CreateDraft(new AgendaDraftRequest(
            Input.MeetingGoal,
            TimeSpan.FromMinutes(Input.DurationMinutes),
            Input.HostNotes,
            Input.MeetingTitle));

        Input.Sections = draft.Sections
            .Select(section => new AgendaSectionInput
            {
                Title = section.Title,
                Purpose = section.Purpose,
                DurationMinutes = (int)section.SuggestedDuration.TotalMinutes,
                FacilitationNotes = section.FacilitationNotes
            })
            .ToList();

        StatusMessage = "Draft agenda generated. Review the sections, adjust timing, then approve it for pacing.";
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(CancellationToken cancellationToken)
    {
        if (Input.Sections.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Generate a draft agenda before approval.");
        }

        var plannedMinutes = Input.Sections.Sum(section => section.DurationMinutes);
        if (plannedMinutes > Input.DurationMinutes)
        {
            ModelState.AddModelError(string.Empty, "Agenda section durations cannot exceed the meeting duration.");
        }

        if (!currentUserContext.TryGetCurrentUser(out var user))
        {
            ModelState.AddModelError(string.Empty, "Sign in before approving the agenda.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var meetingId = Input.MeetingId == Guid.Empty ? Guid.NewGuid() : Input.MeetingId;
        var scheduledStart = DateTimeOffset.UtcNow;
        var meetingSession = new MeetingSession(
            meetingId,
            user!.HostIdentity,
            Input.MeetingTitle,
            scheduledStart,
            scheduledStart.AddMinutes(Input.DurationMinutes),
            MeetingStatus.Scheduled,
            new TeamsMeetingContextIdentifiers(
                string.IsNullOrWhiteSpace(Input.TeamsMeetingId) ? $"local-{meetingId:N}" : Input.TeamsMeetingId,
                Input.TeamsChatId,
                Input.CalendarEventId));

        var existingVersions = await agendaPlanRepository.ListVersionsAsync(meetingId, cancellationToken);
        var plan = new AgendaPlan(
            Guid.NewGuid(),
            meetingId,
            Input.MeetingGoal,
            TimeSpan.FromMinutes(Input.DurationMinutes),
            Input.Sections.Select((section, index) => new AgendaSection(
                section.Title,
                section.Purpose,
                TimeSpan.FromMinutes(section.DurationMinutes),
                index,
                section.FacilitationNotes)),
            existingVersions.Count + 1,
            AgendaApprovalState.Approved,
            user.HostIdentity,
            DateTimeOffset.UtcNow);

        await meetingSessionRepository.SaveAsync(meetingSession, cancellationToken);
        await agendaPlanRepository.SaveAsync(plan, cancellationToken);
        auditLogger.LogAgendaApproved(meetingId, user);

        Input.MeetingId = meetingId;
        IsApproved = true;
        StatusMessage = "Agenda approved and saved. The side panel can now use this plan for pacing.";
        return Page();
    }

    public IActionResult OnPostAddSection()
    {
        ModelState.Clear();
        var remainingMinutes = Math.Max(5, Input.DurationMinutes - Input.Sections.Sum(section => section.DurationMinutes));
        Input.Sections.Add(new AgendaSectionInput
        {
            Title = "New section",
            Purpose = "Describe what this part of the meeting should accomplish.",
            DurationMinutes = Math.Min(remainingMinutes, 10),
            FacilitationNotes = "Clarify the host cue for this section."
        });
        StatusMessage = "Section added. Adjust the title, purpose, and timing before approval.";
        return Page();
    }

    public IActionResult OnPostRemoveSection(int index)
    {
        ModelState.Clear();
        if (index >= 0 && index < Input.Sections.Count)
        {
            Input.Sections.RemoveAt(index);
            StatusMessage = "Section removed. Review the remaining timing before approval.";
        }

        return Page();
    }

    public IActionResult OnPostMoveSectionUp(int index)
    {
        ModelState.Clear();
        if (index > 0 && index < Input.Sections.Count)
        {
            (Input.Sections[index - 1], Input.Sections[index]) = (Input.Sections[index], Input.Sections[index - 1]);
            StatusMessage = "Section moved up.";
        }

        return Page();
    }

    public IActionResult OnPostMoveSectionDown(int index)
    {
        ModelState.Clear();
        if (index >= 0 && index < Input.Sections.Count - 1)
        {
            (Input.Sections[index + 1], Input.Sections[index]) = (Input.Sections[index], Input.Sections[index + 1]);
            StatusMessage = "Section moved down.";
        }

        return Page();
    }

    public sealed class SetupInput
    {
        public Guid MeetingId { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(160, MinimumLength = 3)]
        [Display(Name = "Meeting title")]
        public string MeetingTitle { get; set; } = "Untitled pilot meeting";

        [Required]
        [StringLength(240, MinimumLength = 8)]
        [Display(Name = "Meeting goal")]
        public string MeetingGoal { get; set; } = string.Empty;

        [Range(15, 240)]
        [Display(Name = "Duration (minutes)")]
        public int DurationMinutes { get; set; } = 30;

        [StringLength(2000)]
        [Display(Name = "Optional host notes")]
        public string? HostNotes { get; set; }

        public string? TeamsMeetingId { get; set; }

        public string? TeamsChatId { get; set; }

        public string? CalendarEventId { get; set; }

        public List<AgendaSectionInput> Sections { get; set; } = [];
    }

    public sealed class AgendaSectionInput
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(280, MinimumLength = 4)]
        public string Purpose { get; set; } = string.Empty;

        [Range(1, 240)]
        public int DurationMinutes { get; set; }

        [StringLength(500)]
        public string? FacilitationNotes { get; set; }
    }
}
