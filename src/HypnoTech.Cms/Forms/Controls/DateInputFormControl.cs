using System;

namespace HypnoTech.Cms.Forms.Controls
{
    public class DateInputFormControl : FormControl, IHasValue
    {
        public string Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public DateInputFormControl(FormMemberMapping mapping) : base(mapping)
        {

        }

        public DateInputFormControl(FormMemberMapping mapping, object obj) : base(mapping)
        {
            Value = (mapping.GetPropertyValue(obj) as DateTime?)?.ToString("yyyy-MM-dd");
            if (Value == "0001-01-01")
            {
                Value = string.Empty;
            }
        }

        public override string EditorTemplate => "DateInput";
    }
}
