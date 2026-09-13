namespace Prognize.Api.Domain;

public class AvailabilityWindow
{
    public DayOfWeek Day { get; set; }
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
}