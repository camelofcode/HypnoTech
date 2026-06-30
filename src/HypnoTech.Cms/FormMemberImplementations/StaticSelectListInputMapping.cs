using System.Collections.Generic;
using System.Reflection;
using HypnoTech.Cms.Forms.Controls;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class StaticSelectListInputMapping : FormMemberMapping
    {
        public List<SelectListItem> Values { get; private set; }

        public StaticSelectListInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.StaticSelectList;
        }

        public override FormControl GetControl()
        {
            return new StaticSelectListInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new StaticSelectListInputFormControl(this, obj);
        }

        public StaticSelectListInputMapping SetListOptions(List<SelectListItem> values)
        {
            Values = values;
            return this;
        }
    }
}
