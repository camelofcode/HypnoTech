using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class TextInputMapping : FormMemberMapping
    {
        public TextInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.Text;
        }


        public override FormControl GetControl()
        {
            return new TextInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new TextInputFormControl(this, obj);
        }
    }
}
