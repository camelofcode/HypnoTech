using System;

namespace HypnoTech.Frontend.Attributes
{
    public class NavigationLinkAttribute : Attribute
    {
        public string Id { get; set; }

        public NavigationLinkAttribute(string id)
        {
            Id = id;
        }
    }
}
