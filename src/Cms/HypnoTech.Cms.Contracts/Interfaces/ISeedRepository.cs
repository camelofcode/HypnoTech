namespace HypnoTech.Cms.Contracts.Interfaces
{
    public interface ISeedRepository
    {
        void Seed<T>(string collectionName, string fileName);
    }
}
