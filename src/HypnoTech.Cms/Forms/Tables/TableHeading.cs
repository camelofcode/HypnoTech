using System.Reflection;
using HypnoTech.Common;

namespace HypnoTech.Cms.Forms.Tables
{
    public class TableHeading
    {
        public MemberInfo MemberInfo { get; }

        public string Name { get; set; }

        public SortType Sort { get; set; }

        public string SortClass
        {
            get
            {
                switch (Sort)
                {
                    case SortType.Unsorted:
                        return "fa fa-sort";
                    case SortType.Ascending:
                        return "fa fa-sort-up";
                    case SortType.Descending:
                        return "fa fa-sort-down";
                    default:
                        return "fa fa-sort";
                }
            }
        }

        public TableHeading(string name, MemberInfo memberInfo)
        {
            Name = name;
            MemberInfo = memberInfo;
        }

        public TableHeading(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }
    }
}
