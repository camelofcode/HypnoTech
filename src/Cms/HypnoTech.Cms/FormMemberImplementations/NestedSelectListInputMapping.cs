using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class NestedSelectListInputMapping : FormMemberMapping
    {
        public Type ListType { get; private set; }

        public MemberInfo KeyInfo { get; private set; }

        public string KeyName { get; private set; }

        public MemberInfo TextInfo { get; private set; }

        public string TextName { get; private set; }

        public Type ChildListType { get; private set; }

        public MemberInfo ChildListInfo { get; private set; }

        public string ChildListName { get; private set; }

        public MemberInfo ChildKeyInfo { get; private set; }

        public string ChildKeyName { get; private set; }

        public MemberInfo ChildTextInfo { get; private set; }

        public string ChildTextName { get; private set; }

        public string ChildMemberName { get; set; }

        public Type ChildMemberType { get; set; }

        public MemberInfo ChildMemberInfo { get; set; }

        public string ChildDisplayText { get; private set; }

        private void SetKey(string name, MemberInfo info)
        {
            KeyName = name;
            KeyInfo = info;
        }

        private void SetText(string name, MemberInfo info)
        {
            TextName = name;
            TextInfo = info;
        }

        private void SetChildKey(string name, MemberInfo info)
        {
            ChildKeyName = name;
            ChildKeyInfo = info;
        }

        private void SetChildText(string name, MemberInfo info)
        {
            ChildTextName = name;
            ChildTextInfo = info;
        }

        private void SetChildList(string name, MemberInfo info)
        {
            ChildListName = name;
            ChildListInfo = info;
        }

        public NestedSelectListInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.NestedSelectList;
        }

        public override FormControl GetControl()
        {
            return new NestedSelectListInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new NestedSelectListInputFormControl(this, obj);
        }

        public NestedSelectListInputMapping SetListOptions<TListClass, TKeyMember, TTextMember, TChildListMember, TChildListClass, TChildKeyMember, TChildTextMember>(
            Expression<Func<TListClass, TKeyMember>> keyLambda,
            Expression<Func<TListClass, TTextMember>> textLambda,
            Expression<Func<TListClass, TChildListMember>> childListLambda,
            Expression<Func<TChildListClass, TChildKeyMember>> childKeyLambda,
            Expression<Func<TChildListClass, TChildTextMember>> childTextLambda,
            string childDisplayText)
        {
            ListType = typeof(TListClass);
            MapLambda(keyLambda, SetKey);
            MapLambda(textLambda, SetText);
            MapLambda(childListLambda, SetChildList);
            ChildListType = typeof(TChildListClass);
            MapLambda(childKeyLambda, SetChildKey);
            MapLambda(childTextLambda, SetChildText);
            ChildDisplayText = childDisplayText;
            return this;
        }

        private void MapLambda<TListClass, TKeyMember>(Expression<Func<TListClass, TKeyMember>> memberLambda, Action<string, MemberInfo> setter)
        {
            MemberExpression memberExpression = memberLambda.Body.GetMemberExpression();

            var str = memberExpression.ToString().Split('.').Last();
            var info = memberExpression.Member;
            setter(str, info);
        }
    }
}
