using System.Collections.Generic;
using System.Linq;

namespace HypnoTech.Cms.Forms.Tables
{
    public class Table
    {
        public List<TableHeading> Headings { get; set; }

        public List<TableRow> Rows { get; set; }

        public bool CanMoveUp(TableRow row)
        {
            return row != Rows.First();
        }

        public bool CanMoveDown(TableRow row)
        {
            return row != Rows.Last();
        }

        public Table()
        {
            Headings = new List<TableHeading>();
            Rows = new List<TableRow>();
        }
    }
}
