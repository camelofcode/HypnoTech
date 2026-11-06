using System;
using System.Collections.Generic;

namespace HypnoTech.Demo.Models;

public sealed class Event
{
    public string Id { get; set; }

    public string Title { get; set; }

    public DateTime StartsAt { get; set; }

    public string Type { get; set; }

    public string ArtistId { get; set; }

    public string Venue { get; set; }

    public string Summary { get; set; }

    public string Description { get; set; }

    public string TicketPrice { get; set; }

    public string ImagePath { get; set; }

    public string ProgrammePath { get; set; }

    public bool Featured { get; set; }

    public string Reference { get; set; }

    public string Slug { get; set; }

    public List<ScheduleItem> Schedule { get; set; } = new();
}
