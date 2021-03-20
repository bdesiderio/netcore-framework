using System;

namespace Framework.Data.MongoDB
{
    public interface IMongoDbSettings
    {
        string MongoURL { get; set; }
    }

    public class MongoDbSettings : IMongoDbSettings
    {
        public string MongoURL { get; set; }
    }
}
