namespace HypnoTech.Cms.Forms.Controls
{
    public class MoneyInputFormControl : FormControl, IHasValue
    {
        public decimal Value { get; }

        object IHasValue.Value => Value;

        public bool HasValue => Value != 0.0m;

        public MoneyInputFormControl(FormMemberMapping mapping) : base(mapping)
        {

        }

        public MoneyInputFormControl(FormMemberMapping mapping, object obj) : base(mapping)
        {
            var str = mapping.GetPropertyValue(obj)?.ToString();
            Value = string.IsNullOrEmpty(str) ? 0.0m : decimal.Parse(str);
        }

        public override string EditorTemplate => "MoneyInput";


    }
}
