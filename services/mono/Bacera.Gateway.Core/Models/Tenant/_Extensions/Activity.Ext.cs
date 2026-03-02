namespace Bacera.Gateway;

using M = Activity;

partial class Activity : IEntity
{
    public Activity()
    {
        Data = string.Empty;
    }
}

public static class ActivityExtension
{
    public static Activity CreateActivity(this Matter matter, long partyId, ActionTypes action, StateTypes onState = 0,
        StateTypes? toState = null, string? note = null)
        => new()
        {
            PerformedOn = DateTime.UtcNow,
            MatterId = matter.Id,
            OnStateId = (int)onState,
            ToStateId = toState == null ? matter.StateId : (int)toState,
            PartyId = partyId,
            Data = note ?? string.Empty,
            ActionId = (int)action
        };

    public static Matter AddActivity(this Matter matter, long partyId, ActionTypes action, StateTypes onState = 0,
        StateTypes? toState = null, string? note = null)
    {
        matter.Activities.Add(new Activity
        {
            PerformedOn = DateTime.UtcNow,
            MatterId = matter.Id,
            OnStateId = (int)onState,
            ToStateId = toState == null ? matter.StateId : (int)toState,
            PartyId = partyId,
            Data = note ?? string.Empty,
            ActionId = (int)action
        });
        return matter;
    }

    public static Matter AddActivity(this Matter matter, StateTypes onState = 0, StateTypes? toState = null, long partyId = 1, string? note = null)
    {
        matter.Activities.Add(new M
        {
            PerformedOn = DateTime.UtcNow,
            MatterId = matter.Id,
            OnStateId = (int)onState,
            ToStateId = toState == null ? matter.StateId : (int)toState,
            PartyId = partyId,
            Data = note ?? string.Empty,
            ActionId = 0
        });
        return matter;
    }
}