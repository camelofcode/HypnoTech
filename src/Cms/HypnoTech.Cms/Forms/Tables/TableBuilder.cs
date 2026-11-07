using System.Collections.Generic;
using System.Linq;
using HypnoTech.Common;

namespace HypnoTech.Cms.Forms.Tables
{
    public static class TableBuilder
    {
        public static Table BuildTable<TClass>(this FormClassMapping<TClass> mapping, IList<TClass> items, int sortColumn, SortType direction)
        {
            var table = new Table();

            var headingIndex = 0;
            foreach (var member in mapping.MemberMappings.Where(p => p.IncludedInTable).OrderBy(p => p.TableOrder))
            {
                var heading = new TableHeading(member.TableHeading, member.MemberInfo);
                if (headingIndex == sortColumn)
                {
                    heading.Sort = direction;
                }

                table.Headings.Add(heading);
                headingIndex += 1;
            }

            foreach (var item in items)
            {
                var id = item.GetType().GetProperty("Id")?.GetValue(item).ToString();
                var row = new TableRow(id, mapping.GetTitle(item));

                foreach (var member in mapping.MemberMappings.Where(p => p.IncludedInTable).OrderBy(p => p.TableOrder))
                {
                    row.Cells.Add(member.GetFormattedString(member.GetPropertyValue(item)));
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }
}
