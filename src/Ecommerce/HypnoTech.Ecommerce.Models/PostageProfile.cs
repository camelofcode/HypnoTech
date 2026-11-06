namespace HypnoTech.Ecommerce.Models
{
    public class PostageProfile
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public PostageRegion UnitedKingdom { get; set; }

        public PostageRegion EuropeanUnion { get; set; }

        public PostageRegion RestOfWorld1 { get; set; }

        public PostageRegion RestOfWorld2 { get; set; }

        public PostageRegion RestOfWorld3 { get; set; }

        public bool CanDelete { get; set; }
    }
}
