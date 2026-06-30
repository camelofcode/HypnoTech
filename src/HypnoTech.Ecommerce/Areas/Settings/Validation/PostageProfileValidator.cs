using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Areas.Settings.Validation
{
    public static class PostageProfileValidator
    {
        public static string Validate(this PostageProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                return "Postage Profile name is required";
            }

            if (profile.UnitedKingdom.Alone.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            if (profile.UnitedKingdom.WithOthers.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            if (profile.EuropeanUnion.Alone.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            if (profile.EuropeanUnion.WithOthers.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            if (profile.RestOfWorld1.Alone.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            if (profile.RestOfWorld1.WithOthers.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            if (profile.RestOfWorld2.Alone.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            if (profile.RestOfWorld2.WithOthers.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            if (profile.RestOfWorld3.Alone.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            if (profile.RestOfWorld3.WithOthers.Equals(default(decimal)))
            {
                return "All price fields must be populated and be greater than £0.00";
            }

            return string.Empty;
        }
    }
}
