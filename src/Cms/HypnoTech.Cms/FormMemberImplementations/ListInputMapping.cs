using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.Forms.Controls;
using HypnoTech.Cms.Lists;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class ListInputMapping<TClass> : ListInputMapping
    {
        public ICollection<ListMemberMapping> MemberMappings = new List<ListMemberMapping>();

        public string SingularObjectName = "Item";

        public string OrderProperty { get; protected set; }

        public bool Ordered { get; protected set; }

        public string OrderPropertyJs => Ordered ? char.ToLowerInvariant(OrderProperty[0]) + OrderProperty.Substring(1) : string.Empty;

        public ListInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(
            memberInfo, accessPath, classMapping)
        {
            InputType = InputType.List;
        }

        public override FormControl GetControl()
        {
            return new ListInputFormControl<TClass>(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new ListInputFormControl<TClass>(this, obj);
        }

        public override void SetIds(object model, Func<string> idGenerator)
        {
            if (model != null)
            {
                var list = (IList)model;

                foreach (var item in list)
                {
                    var idMemberName = MemberMappings?.FirstOrDefault(p => p.IsId)?.MemberName;
                    if (!string.IsNullOrEmpty(idMemberName))
                    {
                        var idProperty = item.GetType().GetProperty(idMemberName);
                        if (idProperty != null && idProperty.GetValue(item) == null)
                        {
                            idProperty.SetValue(item, idGenerator());
                        }
                    }
                }
            }
        }

        public ListInputMapping<TClass> MapMembers(Action<ListInputMapping<TClass>> classMappingInitializer)
        {
            classMappingInitializer(this);
            return this;
        }

        public ListMemberMapping MapMember(MemberInfo info, InputType type, bool isNameField = false)
        {
            var memberMap = MemberMappings.FirstOrDefault(p => p.MemberInfo == info);
            if (memberMap == null)
            {
                switch (type)
                {
                    case InputType.None:
                        memberMap = new ListMemberMapping(info, type, MemberName) { IsId = true };
                        break;
                    case InputType.Text:
                        memberMap = new ListMemberMapping(info, type, MemberName, isNameField);
                        break;
                    case InputType.RichText:
                        memberMap = new RichTextListMemberMapping(info, type, MemberName);
                        break;
                    case InputType.StaticSelectList:
                        memberMap = new StaticSelectListListMemberMapping(info, type, MemberName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                MemberMappings.Add(memberMap);
            }

            return memberMap;
        }

        public ListMemberMapping MapId<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(GetMemberInfoFromLambda(memberLambda), InputType.None);
        }

        public ListMemberMapping MapTextInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda, bool isNameField = false)
        {
            return MapMember(GetMemberInfoFromLambda(memberLambda), InputType.Text, isNameField);
        }

        public RichTextListMemberMapping MapRichTextInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(GetMemberInfoFromLambda(memberLambda), InputType.RichText) as RichTextListMemberMapping;
        }

        public StaticSelectListListMemberMapping MapStaticSelectListMemberMapping<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(GetMemberInfoFromLambda(memberLambda), InputType.StaticSelectList) as StaticSelectListListMemberMapping;
        }

        public ListInputMapping<TClass> SetSingularObjectName(string name)
        {
            SingularObjectName = name;
            return this;
        }

        public ListInputMapping<TClass> CanBeOrdered<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            Ordered = true;
            OrderProperty = GetMemberInfoFromLambda(memberLambda).Name;

            return this;
        }

        private static MemberInfo GetMemberInfoFromLambda<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            MemberExpression memberExpression = memberLambda.Body.GetMemberExpression();

            var memberInfo = memberExpression.Member;
            return memberInfo;
        }
    }

    public abstract class ListInputMapping : FormMemberMapping
    {
        protected ListInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMap) : base(memberInfo, accessPath, classMap)
        {
        }

        public abstract void SetIds(object model, Func<string> idGenerator);
    }
}
