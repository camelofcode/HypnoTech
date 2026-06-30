namespace HypnoTech.MongoDb
{
    public class MongoOptions
    {
        public bool UseMongo { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }

        public string ConnectionString => $"mongodb://{Server}:{Port}";
    }
}
