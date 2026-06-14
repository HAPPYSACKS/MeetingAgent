namespace MeetingAgent.Application.Agenda;

public interface IAgendaDraftingService
{
    AgendaDraft CreateDraft(AgendaDraftRequest request);
}
