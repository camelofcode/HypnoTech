using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HypnoTech.Cms.Lists
{
    public class StaticSelectListListMemberMapping : ListMemberMapping
    {
        public List<SelectListItem> Values { get; private set; }

        public StaticSelectListListMemberMapping(MemberInfo info, InputType type, string parentName) : base(info, type, parentName)
        {

        }

        public StaticSelectListListMemberMapping SetListOptions(List<SelectListItem> values)
        {
            Values = values;
            return this;
        }
    }
}
