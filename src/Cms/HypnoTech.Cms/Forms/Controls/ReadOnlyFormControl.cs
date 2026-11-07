using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms.Controls
{
    public class ReadOnlyFormControl : FormControl
    {
        public string Value { get; }

        public ReadOnlyFormControl(ReadOnlyMapping mapping) : base(mapping)
        {

        }

        public ReadOnlyFormControl(ReadOnlyMapping mapping, object obj) : base(mapping)
        {
            Value = mapping.GetPropertyValue(obj).ToString();
        }

        public override string EditorTemplate => "ReadOnly";
    }
}
