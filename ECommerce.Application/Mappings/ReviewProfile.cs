using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Core.Entities;

namespace ECommerce.Application.Mappings
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.CreatedAtFormatted,
                           opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd HH:mm")));
        }
    }
}
