using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class RichTextInputMapping : FormMemberMapping
    {
        public string TinyMceOptions { get; private set; }

        public RichTextInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.RichText;
            TinyMceOptions = @"payment: ['paste', 'link']";
        }

        public override FormControl GetControl()
        {
            return new RichTextInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new RichTextInputFormControl(this, obj);
        }

        public RichTextInputMapping SetTinyMceOptions(string options)
        {
            TinyMceOptions = options;
            return this;
        }
    }
}
