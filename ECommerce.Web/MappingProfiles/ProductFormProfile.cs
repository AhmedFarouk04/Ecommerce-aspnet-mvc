using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.MappingProfiles
{
    public class ProductFormProfile : Profile
    {
        public ProductFormProfile()
        {
            CreateMap<ProductFormViewModel, ProductDto>()
                .ForMember(d => d.ImageUrl, opt => opt.Ignore())
                .ForMember(d => d.ThumbnailUrl, opt => opt.Ignore());

            CreateMap<ProductDto, ProductFormViewModel>();
        }
    }
}
