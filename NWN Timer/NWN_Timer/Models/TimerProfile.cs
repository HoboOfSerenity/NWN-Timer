namespace NWN_Timer.Models;

public class TimerProfile
{
    public string RoundAmount { get; set; } = "25";
    public string RoundWarning { get; set; } = "2";

    public string TurnAmount { get; set; } = "25";
    public string TurnWarning { get; set; } = "2";

    public string HourAmount { get; set; } = "5";
    public string HourWarning { get; set; } = "2";

    public string CustomAmount { get; set; } = "10";
    public string CustomWarning { get; set; } = "2";

    public string CustomUnit { get; set; } = "T";

    public string RoundSound { get; set; } = "None";
    public string TurnSound { get; set; } = "None";
    public string HourSound { get; set; } = "None";
    public string CustomSound { get; set; } = "None";
}