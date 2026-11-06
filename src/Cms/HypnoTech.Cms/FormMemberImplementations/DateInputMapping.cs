using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class DateInputMapping : FormMemberMapping
    {
        public DateInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.Date;
        }

        public override FormControl GetControl()
        {
            return new DateInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new DateInputFormControl(this, obj);
        }
    }
}
