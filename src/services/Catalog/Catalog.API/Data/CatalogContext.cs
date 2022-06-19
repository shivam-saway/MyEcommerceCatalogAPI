using Catalog.API.Entities;
using MongoDB.Driver;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext
    {
        private readonly IConfiguration configuration;

        public CatalogContext(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            var client = new MongoClient(this.configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(this.configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
            this.Products = database.GetCollection<Product>(this.configuration.GetValue<string>("DatabaseSettings:CollectionName"));
        }
        public IMongoCollection<Product> Products { get; };
    }
}
