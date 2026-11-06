namespace HypnoTech.Cms.Forms.Controls
{
    public class YouTubeInputFormControl : FormControl, IHasValue
    {
        public string Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public string ViewModelName => $"modelFor{Name}";

        public string Anchor => $"{Id}-anchor";

        public YouTubeInputFormControl(FormMemberMapping mapping) : base(mapping)
        {

        }

        public YouTubeInputFormControl(FormMemberMapping mapping, object obj) : base(mapping)
        {
           Value = mapping.GetPropertyValue(obj)?.ToString();
        }

        public override string EditorTemplate => "YouTubeInput";
    }
}
