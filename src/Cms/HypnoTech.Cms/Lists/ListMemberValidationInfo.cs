namespace HypnoTech.Cms.Lists
{
    public class ListMemberValidationInfo
    {
        public string Name { get; }

        public string DisplayName { get; }

        public string Id { get; }

        public bool IsRequired { get; }

        public bool IsPlural { get; }

        public bool ValidateUrl { get; }

        public ListMemberValidationInfo(ListMemberMapping mapping)
        {
            DisplayName = mapping.MemberName;
            Name = mapping.MemberNameJs;
            Id = mapping.Id;
            IsRequired = mapping.IsRequired;
            ValidateUrl = mapping.ValidateUrl;
            IsPlural = mapping.NameIsPlural;
        }
    }
}
