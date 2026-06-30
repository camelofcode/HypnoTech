using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class MoneyInputMapping : FormMemberMapping
    {
        public MoneyInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.Money;
        }

        public override FormControl GetControl()
        {
            return new MoneyInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new MoneyInputFormControl(this, obj);
        }
    }
}
