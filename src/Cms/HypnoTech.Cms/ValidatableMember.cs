using System;

namespace HypnoTech.Cms
{
    public abstract class ValidatableMember
    {
        public bool NameIsPlural { get; private set; }

        public bool IsRequired { get; private set; }

        public bool ValidateUrl { get; private set; }

        public Func<string, string> CustomStringValidator { get; private set; }

        public Func<DateTime, string> CustomDateValidator { get; private set; }

        public Func<decimal, string> CustomNumberValidator { get; private set; }

        public ValidatableMember Required(bool plural = false)
        {
            IsRequired = true;
            NameIsPlural = plural;
            return this;
        }

        public ValidatableMember WithUrlValidator()
        {
            ValidateUrl = true;
            return this;
        }

        public void WithCustomStringValidator(Func<string, string> validator)
        {
            CustomStringValidator = validator;
        }

        public void WithCustomNumberValidator(Func<decimal, string> validator)
        {
            CustomNumberValidator = validator;
        }

        public void WithCustomDateValidator(Func<DateTime, string> validator)
        {
            CustomDateValidator = validator;
        }
    }
}
