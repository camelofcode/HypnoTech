using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms
{
    public class AddForm : Form
    {
        public AddForm(FormClassMapping classMapping, object obj = null) : base(classMapping)
        {
            foreach (var input in classMapping.MemberMappings.Where(p => !(p is NoInputMapping) && !p.IsId).OrderBy(p => p.FormOrder))
            {
                Controls.Add(obj == null ? input.GetControl() : input.GetControl(obj));
            }
        }

        protected override string Action => "Add";
    }
}
