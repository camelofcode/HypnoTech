using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms
{
    public class DuplicateForm : Form
    {
        public DuplicateForm(FormClassMapping classMapping, object obj = null) : base(classMapping)
        {
            foreach (var input in classMapping.MemberMappings.Where(p => !(p is NoInputMapping)).OrderBy(p => p.FormOrder))
            {
                if (input.IsId)
                {
                    Controls.Add(input.GetControl());
                }
                else
                {
                    Controls.Add(obj == null ? input.GetControl() : input.GetControl(obj));
                }
            }
        }

        protected override string Action => "Duplicate";
    }
}
