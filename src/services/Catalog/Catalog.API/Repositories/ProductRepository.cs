using Catalog.API.Data;
using Catalog.API.Entities;
using MongoDB.Driver;

namespace Catalog.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ICatalogContext catalog;

        public ProductRepository(ICatalogContext catalog)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }
        public async Task CreateProduct(Product product)
        {
            await catalog.Products.InsertOneAsync(product);
        }

        public async Task<bool> DeleteProduct(string id)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var deletedOrNot = await catalog.Products.DeleteOneAsync(filter);
            return deletedOrNot.IsAcknowledged && deletedOrNot.DeletedCount > 0;
        }

        public async Task<Product> GetProduct(string id)
        {
            return await catalog.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetProductByCategory(string categoryName)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Category, categoryName);
            return await catalog.Products.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductByName(string name)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Name, name);
            return await catalog.Products.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await catalog.Products.Find(p => true).ToListAsync();
        }

        public async Task<bool> UpdateProduct(Product product)
        {
            var updatedOrNot = await catalog.Products.ReplaceOneAsync(filter: g => g.Id == product.Id, replacement: product);
            return updatedOrNot.IsAcknowledged && updatedOrNot.ModifiedCount > 0;
        }
    }
}
