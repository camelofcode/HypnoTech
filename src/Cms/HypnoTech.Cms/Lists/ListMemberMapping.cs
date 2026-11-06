using System;
using System.Reflection;

namespace HypnoTech.Cms.Lists
{
    public class ListMemberMapping : ValidatableMember
    {
        private readonly string _parentName;

        public string MemberName { get; }

        public string Id => $"{MemberName.ToLowerInvariant()}-input-{_parentName}";

        public string MemberNameJs => char.ToLowerInvariant(MemberName[0]) + MemberName.Substring(1);

        public Type MemberType { get; }

        public MemberInfo MemberInfo { get; set; }

        public InputType InputType { get; set; }

        public bool IsNameField { get; set; }

        public bool IsId { get; set; }

        public ListMemberMapping(MemberInfo info, InputType type, string parentName, bool isNameField = false)
        {
            _parentName = parentName;
            MemberInfo = info;
            MemberName = info.Name;
            IsNameField = isNameField;
            if (info is FieldInfo field)
            {
                MemberType = field.FieldType;
            }
            else if (info is PropertyInfo property)
            {
                MemberType = property.PropertyType;
            }
            InputType = type;
        }
    }
}
