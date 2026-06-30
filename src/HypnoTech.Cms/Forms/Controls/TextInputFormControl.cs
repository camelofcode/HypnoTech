namespace HypnoTech.Cms.Forms.Controls
{
    public class TextInputFormControl : FormControl, IHasValue
    {
        public string Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public TextInputFormControl(FormMemberMapping mapping) : base(mapping)
        {

        }

        public TextInputFormControl(FormMemberMapping mapping, object obj) : base(mapping)
        {
           Value = mapping.GetPropertyValue(obj)?.ToString();
        }

        public override string EditorTemplate => "TextInput";
    }
}
