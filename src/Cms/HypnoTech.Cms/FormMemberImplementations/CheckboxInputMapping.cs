using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class CheckboxInputMapping<TClass> : CheckboxInputMapping
    {
        public CheckboxInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(
            memberInfo, accessPath, classMapping)
        {
            InputType = InputType.Toggle;
        }

        public override FormControl GetControl()
        {
            return new CheckboxInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new CheckboxInputFormControl(this, obj);
        }

        public CheckboxInputMapping<TClass> HideWhenChecked<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            MemberExpression memberExpression = memberLambda.Body.GetMemberExpression();
            HiddenWhenChecked.Add(memberExpression.Member.Name);

            return this;
        }

        public CheckboxInputMapping<TClass> HideWhenUnchecked<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            MemberExpression memberExpression = memberLambda.Body.GetMemberExpression();
            HiddenWhenUnchecked.Add(memberExpression.Member.Name);

            return this;
        }
    }

    public abstract class CheckboxInputMapping : FormMemberMapping
    {
        protected CheckboxInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMap) : base(memberInfo, accessPath, classMap)
        {
        }

        public List<string> HiddenWhenChecked = new List<string>();
        public List<string> HiddenWhenUnchecked = new List<string>();
    }
}
