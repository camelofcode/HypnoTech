using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms.Controls
{
    public class CheckboxInputFormControl : FormControl, IHasValue, IProvidesState
    {
        public bool Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => true;

        public readonly List<string> HiddenWhenChecked;
        public readonly List<string> HiddenWhenUnchecked;

        public List<string> HiddenWhenCheckedIds => HiddenWhenChecked.Select(p => $"{p.ToLowerInvariant()}-input").ToList();
        public List<string> HiddenWhenUncheckedIds => HiddenWhenUnchecked.Select(p => $"{p.ToLowerInvariant()}-input").ToList();

        public string ViewModelName => $"modelFor{Name}";

        public CheckboxInputFormControl(CheckboxInputMapping mapping) : base(mapping)
        {
            HiddenWhenChecked = mapping.HiddenWhenChecked;
            HiddenWhenUnchecked = mapping.HiddenWhenUnchecked;
        }

        public CheckboxInputFormControl(CheckboxInputMapping mapping, object obj) : base(mapping)
        {
            HiddenWhenChecked = mapping.HiddenWhenChecked;
            HiddenWhenUnchecked = mapping.HiddenWhenUnchecked;

            Value = mapping.GetPropertyValue(obj) as bool? ?? false;
        }

        public override string EditorTemplate => "CheckboxInput";

        public List<string> HiddenFields => Value ? HiddenWhenChecked : HiddenWhenUnchecked;
    }
}
