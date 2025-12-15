using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Core.Entities;

namespace ECommerce.Application.Mappings
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.ProductImage,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : null));

            CreateMap<Order, OrderDto>();
        }
    }
}
