using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class NoInputMapping : FormMemberMapping
    {
        public NoInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.None;
        }

        public override FormControl GetControl()
        {
            throw new System.Exception("Cannot create input for NoInputMapping");
        }

        public override FormControl GetControl(object obj)
        {
            throw new System.Exception("Cannot create input for NoInputMapping");
        }
    }
}
