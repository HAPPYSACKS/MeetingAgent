using MeetingAgent.Domain.Entities;

namespace MeetingAgent.Application.Pacing;

public interface IPacingEngine
{
    MeetingPacingSnapshot Calculate(MeetingSession meetingSession, AgendaPlan agendaPlan, DateTimeOffset now);
}
