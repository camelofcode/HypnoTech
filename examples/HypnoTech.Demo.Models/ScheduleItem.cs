namespace HypnoTech.Demo.Models;

public sealed class ScheduleItem
{
    public string Id { get; set; }

    public string Time { get; set; }

    public string Title { get; set; }

    public string Format { get; set; }

    public int Order { get; set; }
}
