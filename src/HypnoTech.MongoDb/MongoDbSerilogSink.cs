using System;
using System.Linq;
using HypnoTech.Common.Extensions;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace HypnoTech.MongoDb
{
    public static partial class SerilogSinks
    {
        public static void MongoDb(LoggerConfiguration config, HostBuilderContext context, string siteName)
        {
            var connectionString = $"mongodb://{context.Configuration["Mongo:Server"]}:{context.Configuration["Mongo:Port"]}/{context.Configuration["Mongo:Database"]}";

            config
                .Enrich.WithSiteName(siteName)
                .WriteTo.Sink(
                    new MongoDbLogEventSink(connectionString, "serilog", 50, 1000),
                    LogEventLevel.Warning);
        }
    }

    internal sealed class MongoDbLogEventSink : ILogEventSink
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IMongoDatabase _database;
        private readonly string _collectionName;
        private readonly long _cappedMaxSizeBytes;
        private readonly long _cappedMaxDocuments;
        private readonly object _initializationLock = new object();
        private bool _initialized;

        public MongoDbLogEventSink(
            string connectionString,
            string collectionName,
            int cappedMaxSizeMb,
            long cappedMaxDocuments)
        {
            var mongoUrl = MongoUrl.Create(connectionString);
            var client = new MongoClient(mongoUrl);
            _database = client.GetDatabase(mongoUrl.DatabaseName);
            _collectionName = collectionName;
            _collection = _database.GetCollection<BsonDocument>(collectionName);
            _cappedMaxSizeBytes = cappedMaxSizeMb * 1024L * 1024L;
            _cappedMaxDocuments = cappedMaxDocuments;
        }

        public void Emit(LogEvent logEvent)
        {
            try
            {
                EnsureCollection();

                var properties = new BsonDocument(
                    logEvent.Properties.Select(property =>
                        new BsonElement(property.Key, property.Value.ToString().Trim('"'))));

                var document = new BsonDocument
                {
                    { "Timestamp", logEvent.Timestamp.UtcDateTime },
                    { "Level", logEvent.Level.ToString() },
                    { "MessageTemplate", logEvent.MessageTemplate.Text },
                    { "RenderedMessage", logEvent.RenderMessage() },
                    { "Properties", properties }
                };

                if (logEvent.Exception != null)
                {
                    document.Add("Exception", logEvent.Exception.ToString());
                }

                _collection.InsertOne(document);
            }
            catch (Exception exception)
            {
                SelfLog.WriteLine("Unable to write a log event to MongoDB: {0}", exception);
            }
        }

        private void EnsureCollection()
        {
            if (_initialized)
            {
                return;
            }

            lock (_initializationLock)
            {
                if (_initialized)
                {
                    return;
                }

                var collectionExists = _database.ListCollectionNames()
                    .ToList()
                    .Contains(_collectionName, StringComparer.Ordinal);

                if (!collectionExists)
                {
                    try
                    {
                        _database.CreateCollection(
                            _collectionName,
                            new CreateCollectionOptions
                            {
                                Capped = true,
                                MaxSize = _cappedMaxSizeBytes,
                                MaxDocuments = _cappedMaxDocuments
                            });
                    }
                    catch (MongoCommandException exception) when (exception.Code == 48)
                    {
                        // Another process created the collection after the existence check.
                    }
                }

                _initialized = true;
            }
        }
    }
}
