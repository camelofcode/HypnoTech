using System;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Cms.Lists;

namespace HypnoTech.Cms.Forms.Controls
{
    public class ListInputFormControl<TClass> : ListInputFormControl, IHasValue
    {
        public string ListName { get; }

        public List<TClass> Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => Value != null && Value.Any();

        public TClass ObjectTemplate => Activator.CreateInstance<TClass>();

        public string ViewModelName => $"modelFor{Name}";

        public string Anchor => $"{Id}-anchor";

        public string Template => $"{Id}-template";

        public string NameColumnField { get; }

        public ICollection<ListMemberMapping> MemberMappings;

        public ICollection<ListMemberValidationInfo> Validation => MemberMappings.Select(p => new ListMemberValidationInfo(p)).ToList();

        public string SingularObjectName { get; set; }

        public bool Ordered { get; set; }

        public string OrderPropertyJs { get; set; }

        public string OrderProperty { get; set; }

        public ListInputFormControl(ListInputMapping<TClass> mapping) : base(mapping)
        {
            ListName = mapping.MemberName;
            NameColumnField = mapping.MemberMappings.FirstOrDefault(p => p.IsNameField)?.MemberNameJs;
            MemberMappings = mapping.MemberMappings;
            SingularObjectName = mapping.SingularObjectName;
            Ordered = mapping.Ordered;
            OrderProperty = mapping.OrderProperty;
            OrderPropertyJs = mapping.OrderPropertyJs;
        }

        public ListInputFormControl(ListInputMapping<TClass> mapping, object obj) : base(mapping)
        {
            Value = (List<TClass>)mapping.GetPropertyValue(obj);
            ListName = mapping.MemberName;
            NameColumnField = mapping.MemberMappings.FirstOrDefault(p => p.IsNameField)?.MemberNameJs;
            MemberMappings = mapping.MemberMappings;
            SingularObjectName = mapping.SingularObjectName;
            Ordered = mapping.Ordered;
            OrderProperty = mapping.OrderProperty;
            OrderPropertyJs = mapping.OrderPropertyJs;
        }

        public override string EditorTemplate => "ListInput";
    }

    public abstract class ListInputFormControl : FormControl
    {
        protected ListInputFormControl(FormMemberMapping mapping) : base(mapping)
        {
        }
    }
}
