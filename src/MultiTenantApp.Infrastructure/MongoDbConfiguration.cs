using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;

namespace MultiTenantApp.Infrastructure
{
    /// <summary>
    /// Configures MongoDB serialization conventions for the application
    /// </summary>
    public static class MongoDbConfiguration
    {
        private static bool _isConfigured = false;

        /// <summary>
        /// Configures MongoDB serialization conventions.
        /// This should be called once during application startup.
        /// </summary>
        public static void Configure()
        {
            if (_isConfigured)
                return;

            // Register Guid serializer with Standard representation
            BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));

            // Register custom conventions
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            };

            ConventionRegistry.Register("CustomConventions", conventionPack, t => true);

            _isConfigured = true;
        }

        /// <summary>
        /// Creates a MongoDB client with proper configuration
        /// </summary>
        public static MongoClient CreateClient(string connectionString)
        {
            Configure();

            var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);

            return new MongoClient(mongoClientSettings);
        }
    }
}