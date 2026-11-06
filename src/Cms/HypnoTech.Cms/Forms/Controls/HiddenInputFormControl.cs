namespace HypnoTech.Cms.Forms.Controls
{
    public class HiddenInputFormControl : FormControl, IHasValue
    {
        public string Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public HiddenInputFormControl(FormMemberMapping mapping) : base(mapping)
        {

        }

        public HiddenInputFormControl(FormMemberMapping mapping, object obj) : base(mapping)
        {
           Value = mapping.GetPropertyValue(obj)?.ToString();
        }

        public override string EditorTemplate => "HiddenInput";
    }
}
