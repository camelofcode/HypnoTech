using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class HiddenInputMapping : FormMemberMapping
    {
        public HiddenInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.Hidden;
        }

        public override FormControl GetControl()
        {
            return new HiddenInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new HiddenInputFormControl(this, obj);
        }
    }
}
