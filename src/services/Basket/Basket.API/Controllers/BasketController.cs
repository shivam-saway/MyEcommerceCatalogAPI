using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Basket.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository basketRepository;
        private readonly DiscountGrpcServices discount;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndPoint;
        public BasketController(IBasketRepository basketRepository, DiscountGrpcServices discount, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            this.basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            this.discount = discount ?? throw new ArgumentNullException(nameof(discount));
            _mapper = mapper;
            _publishEndPoint = publishEndpoint;
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await basketRepository.GetBasket(userName);
            return Ok(basket ?? new ShoppingCart(userName));
        }
        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            foreach (var item in basket.Items)
            {
                var coupon = await discount.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }

            return Ok(await basketRepository.UpdateBasket(basket));
        }
        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string userName)
        {
            await basketRepository.DeleteBasket(userName);
            return Ok();
        }
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            //get existing basket with total price
            var basket = await basketRepository.GetBasket(basketCheckout.UserName ?? "");
            if (basket == null)
            {
                return BadRequest();
            }
            // Create basketCheckoutEvent - SetTotalPrice on basket checkout eventMessage
            // send checkout event to rabbit mq
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = (decimal)basket.TotalPrice;
            await _publishEndPoint.Publish(eventMessage);
            // remove the basket
            await basketRepository.DeleteBasket(basket.UserName ?? "");

            return Accepted();
        }
    }
}
