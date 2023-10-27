namespace ConsoleAppBoilerplate.Configuration;

public class ActivityParamsConfig
{
    public const string Section = "ActivityParams";
    public int MinNumberOfParticipants { get; set; }
    public int MaxNumberOfParticipants { get; set; }
}