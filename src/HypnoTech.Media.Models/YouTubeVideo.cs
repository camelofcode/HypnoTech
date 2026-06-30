namespace HypnoTech.Media.Models
{
    public class YouTubeVideo
    {
        public virtual string Id { get; set; }

        public virtual string Title { get; set; }

        public string YouTubeId { get; set; }

        public string ThumbnailUrl => $"https://img.youtube.com/vi/{YouTubeId}/0.jpg";

        public string VideoUrl => $"https://www.youtube.com/embed/{YouTubeId}?enablejsapi=1&amp;version=3&amp;playerapiid=ytplayer";
    }
}
