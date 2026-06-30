using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HypnoTech.Cms.Forms.Controls
{
    public class NestedSelectListInputFormControl : FormControl
    {
        public string ViewModelName => $"modelFor{Name}";

        public string Anchor => $"{Id}-anchor";

        public List<NestedSelectListItem> Values { get; set; }

        public string Value { get; }

        public string ChildValue { get; }

        public string ChildName { get; }

        public string ChildId => $"{ChildName.ToLowerInvariant()}-input";

        public string ChildLabel { get; }

        public Type ListType { get; }

        private string KeyName { get; }

        private string TextName { get; }

        private Type ChildListType { get; }

        private string ChildListName { get; }

        private string ChildKeyName { get; }

        private string ChildTextName { get; }

        public NestedSelectListInputFormControl(NestedSelectListInputMapping mapping) : base(mapping)
        {
            ListType = mapping.ListType;
            KeyName = mapping.KeyName;
            TextName = mapping.TextName;
            ChildListType = mapping.ChildListType;
            ChildKeyName = mapping.ChildKeyName;
            ChildTextName = mapping.ChildTextName;
            ChildName = mapping.ChildMemberName;
            ChildLabel = mapping.ChildDisplayText;
            ChildListName = mapping.ChildListName;
            Values = new List<NestedSelectListItem>();
        }

        public NestedSelectListInputFormControl(NestedSelectListInputMapping mapping, object obj) : base(mapping)
        {
            ListType = mapping.ListType;
            KeyName = mapping.KeyName;
            TextName = mapping.TextName;
            ChildListType = mapping.ChildListType;
            ChildKeyName = mapping.ChildKeyName;
            ChildTextName = mapping.ChildTextName;
            ChildName = mapping.ChildMemberName;
            ChildLabel = mapping.ChildDisplayText;
            ChildListName = mapping.ChildListName;
            Value = mapping.GetPropertyValue(obj).ToString();
            ChildValue = obj.GetType().GetProperty(ChildName)?.GetValue(obj).ToString();
            Values = new List<NestedSelectListItem>();
        }

        public void MapValues(List<object> valueObjects)
        {
            foreach (var obj in valueObjects)
            {
                string key = GetPropertyValue(obj, KeyName).ToString();
                string text = GetPropertyValue(obj, TextName).ToString();

                var items = new List<SelectListItem>();
                var list = (IList)GetPropertyValue(obj, ChildListName);
                foreach (var item in list)
                {
                    string childKey = GetPropertyValue(item, ChildKeyName).ToString();
                    string childText = GetPropertyValue(item, ChildTextName).ToString();

                    items.Add(new SelectListItem(childKey, childText, childKey == ChildValue));
                }

                Values.Add(new NestedSelectListItem(text, key, key == Value, items));
            }
        }

        public object GetPropertyValue(object obj, string accessPath)
        {
            var propertyNames = accessPath.Split('.');

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

        public override string EditorTemplate => "NestedSelectListInput";
    }

    public class NestedSelectListItem : SelectListItem
    {
        public List<NestedSelectListItem> Children { get; set; }

        public NestedSelectListItem(string text, string value, bool selected) : base(text, value, selected)
        {
            Children = new List<NestedSelectListItem>();
        }

        public NestedSelectListItem(string text, string value, bool selected, List<SelectListItem> children) : base(text, value, selected)
        {
            Children = children.Select(p => new NestedSelectListItem(p.Text, p.Value, p.Selected)).ToList();
        }
    }
}
