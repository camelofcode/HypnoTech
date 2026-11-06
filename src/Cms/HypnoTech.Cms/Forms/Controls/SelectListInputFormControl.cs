using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HypnoTech.Cms.Forms.Controls
{
    public class SelectListInputFormControl : FormControl, IHasValue
    {
        public Type ListType { get; }

        public string Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public List<SelectListItem> Values { get; }

        private Type SubClassType { get; }

        private bool IsOptional { get; }

        private string KeyName { get; }

        private string TextName { get; }

        private string SubClassTextName { get; }

        private string ListName { get; }

        public SelectListInputFormControl(SelectListInputMapping mapping) : base(mapping)
        {
            ListType = mapping.ListType;
            KeyName = mapping.KeyName;
            TextName = mapping.TextName;
            SubClassType = mapping.SubClassType;
            SubClassTextName = mapping.SubClassTextName;
            IsOptional = mapping.IsOptional;
            ListName = mapping.ListName;
            Values = new List<SelectListItem>();
        }

        public SelectListInputFormControl(SelectListInputMapping mapping, object obj) : base(mapping)
        {
            ListType = mapping.ListType;
            KeyName = mapping.KeyName;
            TextName = mapping.TextName;
            SubClassType = mapping.SubClassType;
            SubClassTextName = mapping.SubClassTextName;
            ListName = mapping.ListName;
            IsOptional = mapping.IsOptional;
            Values = new List<SelectListItem>();
            Value = mapping.GetPropertyValue(obj)?.ToString();
        }

        public void MapValues(List<object> valueObjects)
        {
            if (IsOptional)
            {
                Values.Add(new SelectListItem("None", null));
            }

            foreach (var obj in valueObjects)
            {

                if (SubClassType == null)
                {
                    string key = GetPropertyValue(obj, KeyName).ToString();
                    string text = GetPropertyValue(obj, TextName).ToString();
                    Values.Add(new SelectListItem(text, key, key == Value));
                }
                else
                {
                    var list = (IList) GetPropertyValue(obj, ListName);
                    foreach (var subObj in list)
                    {
                        string key = GetPropertyValue(subObj, KeyName).ToString();
                        string text = SubClassTextName != null
                            ? $"{GetPropertyValue(obj, TextName)} - {GetPropertyValue(subObj, SubClassTextName)}"
                            : GetPropertyValue(subObj, TextName).ToString();

                        Values.Add(new SelectListItem(text, key, key == Value));
                    }
                }

                if (!Values.Any(p => p.Selected))
                {
                    Values.First().Selected = true;
                }
            }
        }

        private object GetPropertyValue(object obj, string accessPath)
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

        public override string EditorTemplate => "SelectListInput";
    }
}
