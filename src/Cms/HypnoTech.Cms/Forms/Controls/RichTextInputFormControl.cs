using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms.Controls
{
    public class RichTextInputFormControl : FormControl, IHasValue
    {
        public string Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public string Options { get; }

        public RichTextInputFormControl(RichTextInputMapping mapping) : base(mapping)
        {
            Options = mapping.TinyMceOptions;
        }

        public RichTextInputFormControl(RichTextInputMapping mapping, object obj) : base(mapping)
        {
            Value = mapping.GetPropertyValue(obj)?.ToString();
            Options = mapping.TinyMceOptions;
        }

        public override string EditorTemplate => "RichTextInput";


    }
}
