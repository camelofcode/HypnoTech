namespace HypnoTech.Cms.Mvc
{
    public class AdminSettingsLink
    {
        public string Page { get; set; }

        public string Text { get; set; }

        public string Icon { get; set; }

        public string Url => $"/Settings/{Page}";

        public string Role { get; set; }
    }
}
