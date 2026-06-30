using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class YouTubeInputMapping : FormMemberMapping
    {
        public YouTubeInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.YouTube;
        }

        public override FormControl GetControl()
        {
            return new YouTubeInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new YouTubeInputFormControl(this, obj);
        }
    }
}
