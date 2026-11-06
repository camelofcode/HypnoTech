namespace HypnoTech.Demo.Models;

public sealed class Artist
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Discipline { get; set; }

    public string Biography { get; set; }

    public string ImagePath { get; set; }

    public string Website { get; set; }

    public bool Featured { get; set; }

    public int Order { get; set; }
}
