using System;
using System.Collections.Generic;
using System.Linq;

namespace HypnoTech.Media.Models
{
    public class PhotoAlbum
    {
        public virtual string Id { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public List<Photo> Photos { get; set; }

        public string CoverPhoto => Photos.FirstOrDefault()?.ImagePath;
    }
}
