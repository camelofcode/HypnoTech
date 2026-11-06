using System;

namespace HypnoTech.Media.Models
{
    public class AudioTrack
    {
        public virtual string Id { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }

        public virtual string RelatedId { get; set; }

        //Not mapped in db, will be set from related item via AudioTrackEnricher
        public string RelatedName { get; set; }

        //Not mapped in db, will be set from related item via AudioTrackEnricher
        public string ImagePath { get; set; }

        //Not mapped in db, will be set from related item via AudioTrackEnricher
        public string Url { get; set; }

        //Not mapped in db, will be set from related item via AudioTrackEnricher
        public DateTime Date { get; set; }
    }
}
