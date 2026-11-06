using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class TextAreaInputMapping : FormMemberMapping
    {
        public int Rows { get; set; }

        public TextAreaInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.Text;
            Rows = 4;
        }

        public override FormControl GetControl()
        {
            return new TextAreaInputFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new TextAreaInputFormControl(this, obj);
        }

        public TextAreaInputMapping SetRows(int rows)
        {
            Rows = rows;
            return this;
        }
    }
}
