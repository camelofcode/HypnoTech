namespace HypnoTech.Common.Models.Settings
{
    public class NavBarLink
    {
        public string Id { get; set; }

        public string Key { get; set; }

        public string Text { get; set; }

        public int Order { get; set; }

        public bool Enabled { get; set; }

        public string Url { get; set; }

        public NavBarLink(string key, string text)
        {
            Key = key;
            Text = text;
        }

        public NavBarLink()
        {
            
        }
    }
}