using Discount.Grpc.Protos;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcServices
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient discount;

        public DiscountGrpcServices(DiscountProtoService.DiscountProtoServiceClient discount)
        {
            this.discount = discount ?? throw new ArgumentNullException(nameof(discount));
        }
        public async Task<CouponModel> GetDiscount(string? productName)
        {
            var discountRequest = new GetDiscountRequest { ProductName = productName };
            return await discount.GetDiscountAsync(discountRequest);
        }
    }
}
