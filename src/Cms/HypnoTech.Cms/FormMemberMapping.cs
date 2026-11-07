using System;
using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms
{
    public abstract class FormMemberMapping : ValidatableMember
    {
        private readonly FormClassMapping _classMap;

        public string MemberName { get; }

        public Type MemberType { get; }

        public MemberInfo MemberInfo { get; set; }

        public string AccessPath { get; set; }

        protected InputType InputType { get; set; }

        public bool IsList => InputType == InputType.List;

        public bool IncludedInTable { get; protected set; }

        public string TableHeading { get; protected set; }

        public int? TableOrder { get; private set; }

        public int? FormOrder { get; private set; }

        public bool IsId => MemberName == "Id";

        public bool IsSearchable { get; private set; }

        protected bool IsReadOnly => IsId;

        public string DisplayText { get; private set; }

        private Func<object, string> _stringFormatter;

        public abstract FormControl GetControl();

        public abstract FormControl GetControl(object obj);

        public bool IsFileInput => InputType == InputType.FileUpload || InputType == InputType.ImageUpload || InputType == InputType.PhotoGallery;

        public string GetFormattedString(object value)
        {
            if (_stringFormatter == null)
            {
                return value.ToString();
            }

            return _stringFormatter.Invoke(value);
        }

        public object GetPropertyValue(object obj)
        {
            var propertyNames = AccessPath.Split('.');

            foreach (var t in propertyNames)
            {
                if (obj != null)
                {
                    var propertyInfo = obj.GetType().GetProperty(t);
                    obj = propertyInfo != null ? propertyInfo.GetValue(obj) : null;
                }
            }

            return obj;
        }

        protected FormMemberMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMap)
        {
            MemberInfo = memberInfo;
            AccessPath = accessPath;
            _classMap = classMap;
            MemberName = memberInfo.Name;
            if (memberInfo is FieldInfo field)
            {
                MemberType = field.FieldType;
            }
            else if (memberInfo is PropertyInfo property)
            {
                MemberType = property.PropertyType;
            }
        }

        public FormMemberMapping IncludeInTable(int order)
        {
            return IncludeInTable(MemberName, order);
        }

        public FormMemberMapping IncludeInTable(string heading, int order)
        {
            IncludedInTable = true;
            TableHeading = heading;
            TableOrder = order;
            return this;
        }

        public FormMemberMapping Searchable()
        {
            IsSearchable = true;
            return this;
        }

        public FormMemberMapping WithDisplayText(string text)
        {
            DisplayText = text;
            return this;
        }

        public FormMemberMapping WithStringFormatter(Func<object, string> lambda)
        {
            _stringFormatter = lambda;
            return this;
        }

        public FormMemberMapping WithDateStringFormatter(string formatString = "dd MMM yyyy")
        {
            _stringFormatter = value =>
             {
                 if (!(value is DateTime dateTime))
                 {
                     return string.Empty;
                 }

                 return dateTime.ToString("dd MMM yy");
             };
            return this;
        }
    }
}
