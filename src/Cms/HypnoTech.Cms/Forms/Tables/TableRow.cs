using System.Collections.Generic;

namespace HypnoTech.Cms.Forms.Tables
{
    public class TableRow
    {
        public List<string> Cells { get; }

        public string Id { get; }

        public string Title { get; set; }

        public TableRow(string id, string title)
        {
            Id = id;
            Title = title;
            Cells = new List<string>();
        }
    }
}
