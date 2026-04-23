
[System.Serializable]
public class DialogueEntry
{
    public string text;
    public AdvanceTrigger trigger = AdvanceTrigger.AnyKey;
    public float autoAdvanceDelay = 2f;
}

//different ways to advance dialogue 
public enum AdvanceTrigger
{
    //for when the player presses a specific key 
    AnyKey,

    //for time out auto-advance 
    TimedAuto,

    //for in-engine triggers
    ExternalEvent,
}