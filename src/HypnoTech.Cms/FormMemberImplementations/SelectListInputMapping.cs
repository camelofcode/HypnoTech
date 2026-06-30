using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class SelectListInputMapping : FormMemberMapping
    {
        public Type ListType { get; private set; }

        public Type SubClassType { get; private set; }

        public MemberInfo KeyInfo { get; private set; }

        public string KeyName { get; private set; }

        public MemberInfo TextInfo { get; private set; }

        public string TextName { get; private set; }

        public MemberInfo ListInfo { get; private set; }

        public string ListName { get; private set; }

        public MemberInfo SubClassTextInfo { get; private set; }

        public string SubClassTextName { get; private set; }

        public bool IsOptional { get; private set; }

        private void SetList(string name, MemberInfo info)
        {
            ListName = name;
            ListInfo = info;
        }

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

        private void SetSubClassText(string name, MemberInfo info)
        {
            SubClassTextName = name;
            SubClassTextInfo = info;
        }

        public SelectListInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.SelectList;
        }

        public override FormControl GetControl()
        {
            return new SelectListInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new SelectListInputFormControl(this, obj);
        }

        public SelectListInputMapping SetListOptions<TListClass, TKeyMember, TTextMember>(
            Expression<Func<TListClass, TKeyMember>> keyLambda, Expression<Func<TListClass, TTextMember>> textLambda, bool optional = false)
        {
            ListType = typeof(TListClass);
            MapKeyLambda(keyLambda);
            MapTextLambda(textLambda);
            IsOptional = optional;
            return this;
        }

        public SelectListInputMapping SetListOptions<TListClass, TListMember, TSubClass, TKeyMember, TTextMember>(
            Expression<Func<TListClass, TListMember>> listLambda,
            Expression<Func<TSubClass, TKeyMember>> keyLambda, 
            Expression<Func<TSubClass, TTextMember>> textLambda, 
            bool optional = false)
        {
            ListType = typeof(TListClass);
            SubClassType = typeof(TSubClass);
            MapLambda(listLambda, SetList);
            MapLambda(keyLambda, SetKey);
            MapLambda(textLambda, SetText);
            IsOptional = optional;
            return this;
        }

        public SelectListInputMapping SetListOptions<TListClass, TListMember, TSubClass, TKeyMember, TTextMember>(
            Expression<Func<TListClass, TListMember>> listLambda,
            Expression<Func<TSubClass, TKeyMember>> keyLambda, 
            Expression<Func<TListClass, TTextMember>> listClassTextLambda, 
            Expression<Func<TSubClass, TTextMember>> subClassLTextLambda, 
            bool optional = false)
        {
            ListType = typeof(TListClass);
            SubClassType = typeof(TSubClass);
            MapLambda(listLambda, SetList);
            MapLambda(keyLambda, SetKey);
            MapLambda(listClassTextLambda, SetText);
            MapLambda(subClassLTextLambda, SetSubClassText);
            IsOptional = optional;
            return this;
        }

        private void MapKeyLambda<TListClass, TKeyMember>(Expression<Func<TListClass, TKeyMember>> memberLambda)
        {
            MemberExpression memberExpression = memberLambda.Body.GetMemberExpression();

            KeyName = string.Join('.', memberExpression.ToString().Split('.').Skip(1));
            KeyInfo = memberExpression.Member;
        }

        private void MapTextLambda<TListClass, TTextMember>(Expression<Func<TListClass, TTextMember>> memberLambda)
        {
            MemberExpression memberExpression = memberLambda.Body.GetMemberExpression();

            TextName = string.Join('.', memberExpression.ToString().Split('.').Skip(1));
            TextInfo = memberExpression.Member;
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
