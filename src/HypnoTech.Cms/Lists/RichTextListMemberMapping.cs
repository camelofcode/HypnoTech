using System.Reflection;

namespace HypnoTech.Cms.Lists
{
    public class RichTextListMemberMapping : ListMemberMapping
    {
        public string TinyMceOptions { get; private set; }

        public RichTextListMemberMapping(MemberInfo info, InputType type, string parentName) : base(info, type, parentName)
        {

        }

        public RichTextListMemberMapping SetTinyMceOptions(string tinyMceOptions)
        {
            TinyMceOptions = tinyMceOptions;
            return this;
        }
    }
}
