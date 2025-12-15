using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Core.Entities;

namespace ECommerce.Application.Mappings
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.CategoryName,
                    m => m.MapFrom(s => s.Category != null ? s.Category.Name : null));

            CreateMap<ProductDto, Product>();
        }
    }
}
