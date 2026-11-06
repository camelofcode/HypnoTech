using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HypnoTech.Cms.Forms.Controls
{
    public class StaticSelectListInputFormControl : FormControl, IHasValue
    {
        public string Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public List<SelectListItem> Values { get; }

        public StaticSelectListInputFormControl(StaticSelectListInputMapping mapping) : base(mapping)
        {
            Values = mapping.Values;
            Values.First().Selected = true;
        }

        public StaticSelectListInputFormControl(StaticSelectListInputMapping mapping, object obj) : base(mapping)
        {
            Values = mapping.Values;
            Value = mapping.GetPropertyValue(obj)?.ToString();
            Values.First(p => p.Value == Value).Selected = true;
        }

        public override string EditorTemplate => "StaticSelectListInput";
    }
}
