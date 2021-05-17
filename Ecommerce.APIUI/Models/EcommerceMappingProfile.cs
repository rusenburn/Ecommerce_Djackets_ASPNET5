using AutoMapper;
using Ecommerce.APIUI.Dtos;
using Ecommerce.Shared.Entities;

namespace Ecommerce.APIUI.Models
{
    public class EcommerceMappingProfile : Profile
    {
        public EcommerceMappingProfile()
        {
            CreateMap<OrderInputDto, Order>();
            CreateMap<Order, OrderInputDto>();

            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<OrderItemDto, OrderItem>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product == null ? 0 : src.Product.Id));

            CreateMap<Product,ProductDto>();

            CreateMap<Category,CategoryDTO>();
                
        }
    }
}