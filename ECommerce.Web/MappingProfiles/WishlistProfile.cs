using AutoMapper;
using ECommerce.Core.Entities;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Mappings
{
    public class WishlistProfile : Profile
    {
        public WishlistProfile()
        {
            CreateMap<Wishlist, WishlistAdminViewModel>();
        }
    }
}
