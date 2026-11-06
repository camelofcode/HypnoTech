using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms.Controls
{
    public class TextAreaInputFormControl : TextInputFormControl
    {
        public int Rows { get; set; }

        public TextAreaInputFormControl(TextAreaInputMapping mapping) : base(mapping)
        {
            Rows = mapping.Rows;
        }

        public TextAreaInputFormControl(TextAreaInputMapping mapping, object obj) : base(mapping, obj)
        {
            Rows = mapping.Rows;
        }

        public override string EditorTemplate => "TextAreaInput";
    }
}
