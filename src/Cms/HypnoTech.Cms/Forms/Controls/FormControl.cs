namespace HypnoTech.Cms.Forms.Controls
{
    public abstract class FormControl
    {
        public string Name => Mapping.MemberInfo.Name;

        public string Id => $"{Name.ToLowerInvariant()}-input";

        public string Label { get; }

        public abstract string EditorTemplate { get; }

        public FormMemberMapping Mapping { get; set; }

        protected FormControl(FormMemberMapping mapping)
        {
            Mapping = mapping;
            Label = string.IsNullOrEmpty(mapping.DisplayText) ? Name : mapping.DisplayText;
        }

        public string ValidationMessage { get; set; }

        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);
    }
}
