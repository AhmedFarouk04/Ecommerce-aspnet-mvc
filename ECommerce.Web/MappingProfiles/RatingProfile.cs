using AutoMapper;
using ECommerce.Core.Entities;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.MappingProfiles
{
    public class RatingProfile : Profile
    {
        public RatingProfile()
        {
            CreateMap<Rating, RatingAdminViewModel>();
        }
    }
}
