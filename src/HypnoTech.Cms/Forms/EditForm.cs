using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms
{
    public class EditForm : Form
    {
        public EditForm(FormClassMapping classMapping, object obj = null) : base(classMapping)
        {
            foreach (var input in classMapping.MemberMappings.Where(p => !(p is NoInputMapping)).OrderBy(p => p.FormOrder))
            {
                Controls.Add(obj == null ? input.GetControl() : input.GetControl(obj));
            }
        }

        protected override string Action => "Edit";
    }
}
