namespace HypnoTech.EntityFrameworkCore
{
    public sealed class HypnoTechEntityFrameworkCoreOptions
    {
        public bool ApplyMigrationsAtStartup { get; set; } = true;

        public bool SeedAtStartup { get; set; } = true;
    }
}
