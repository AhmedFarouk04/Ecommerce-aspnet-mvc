using System.Diagnostics;
using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Core.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HomeController(
            ILogger<HomeController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        
        public IActionResult Index()
        {
            var featured = _unitOfWork.Products
                .GetProductsWithCategory()
                .OrderByDescending(p => p.Id)
                .Take(4)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category.Name
                })
                .ToList();

            var categories = _unitOfWork.Categories
    .GetAll()
    .Take(4) 
    .Select(c => new CategoryDto
    {
        Id = c.Id,
        Name = c.Name
    })
    .ToList();

            return View(new HomeViewModel
            {
                FeaturedProducts = featured,
                Categories = categories
            });
        }


        
        public IActionResult Privacy()
        {
            return View();
        }

       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
