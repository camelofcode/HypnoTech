using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class ReadOnlyMapping : FormMemberMapping
    {
        public ReadOnlyMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.ReadOnly;
        }

        public override FormControl GetControl()
        {
            return new ReadOnlyFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new ReadOnlyFormControl(this, obj);
        }
    }
}
