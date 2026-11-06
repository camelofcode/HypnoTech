using System.Collections.Generic;
using System.Linq;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Ecommerce.Frontend.Models
{
    public class CheckoutViewModel
    {
        public List<Country> Countries { get; }

        public CheckoutViewModel(List<Country> countries)
        {
            Countries = new List<Country>
            {
                new Country
                {
                    DisplayText = "Please select your country"
                }
            };
            Countries = Countries.Concat(countries).ToList();
        }
    }
}
