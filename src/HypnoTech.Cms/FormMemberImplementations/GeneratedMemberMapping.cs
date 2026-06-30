using System;
using System.Reflection;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class GeneratedMemberMapping<TClass> : HiddenInputMapping
    {
        private readonly Func<TClass, object> _generator;

        public GeneratedMemberMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping, Func<TClass, object> generator) : base(memberInfo, accessPath, classMapping)
        {
            _generator = generator;

            InputType = InputType.Generated;
        }

        public void Generate(TClass model)
        {
            model.GetType().GetProperty(MemberName)?.SetValue(model, _generator(model));
        }
    }
}
