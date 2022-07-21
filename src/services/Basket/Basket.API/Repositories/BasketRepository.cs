using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache distributedCache;

        public BasketRepository( IDistributedCache distributedCache )
        {
            this.distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }
        public async Task<ShoppingCart?> GetBasket(string userName)
        {
            var basket = await distributedCache.GetStringAsync(userName);

            if (String.IsNullOrEmpty(basket))
                return null;

            return JsonConvert.DeserializeObject<ShoppingCart>(basket);
        }

        public async Task<ShoppingCart?> UpdateBasket(ShoppingCart basket)
        {
            await distributedCache.SetStringAsync(basket.UserName, JsonConvert.SerializeObject(basket));

            return await GetBasket(basket.UserName ?? "");
        }

        public async Task DeleteBasket(string userName)
        {
            await distributedCache.RemoveAsync(userName);
        }
    }
}
