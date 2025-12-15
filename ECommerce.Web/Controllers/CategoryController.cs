using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Identity;
using ECommerce.Web.Services;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly AdminActivityLogger _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public CategoryController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        AdminActivityLogger logger,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
    }

    
    private Category? LoadCategory(int id)
    {
        return _unitOfWork.Categories.GetById(id);
    }


    [AllowAnonymous]
    public IActionResult Index(string? search, int page = 1, int pageSize = 5)
    {
        var query = _unitOfWork.Categories.GetAll();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query
                .Where(c => c.Name.Contains(search));
        }

        int totalCount = query.Count();

        var categories = query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var vm = new CategoryListViewModel
        {
            Categories = _mapper.Map<IEnumerable<CategoryDto>>(categories),
            Search = search,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return View(vm);
    }




    public IActionResult Create() => View(new CategoryDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        bool exists = _unitOfWork.Categories
            .GetAll()
            .Any(c => c.Name.ToLower() == dto.Name.ToLower());

        if (exists)
        {
            ModelState.AddModelError("Name", "Category name already exists.");
            return View(dto); 
        }

        var category = _mapper.Map<Category>(dto);

        _unitOfWork.Categories.Add(category);
        _unitOfWork.Complete();

        await _logger.LogAsync(
            _userManager.GetUserId(User),
            "CreateCategory",
            $"Created: {category.Name} (ID: {category.Id})"
        );

        return RedirectToAction(nameof(Index)); 
    }



    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var category = LoadCategory(id);
        if (category == null)
            return NotFound();

        return View(_mapper.Map<CategoryDto>(category));
    }

    
    public IActionResult Edit(int id)
    {
        var category = LoadCategory(id);
        if (category == null)
            return NotFound();

        return View(_mapper.Map<CategoryDto>(category));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var category = LoadCategory(dto.Id);
        if (category == null)
            return NotFound();

        _mapper.Map(dto, category);

        _unitOfWork.Categories.Update(category);
        _unitOfWork.Complete();

        await _logger.LogAsync(
            _userManager.GetUserId(User),
            "EditCategory",
            $"Updated: {category.Name} (ID: {category.Id})"
        );

        return RedirectToAction(nameof(Index));
    }

   
    public IActionResult Delete(int id)
    {
        var category = LoadCategory(id);
        if (category == null)
            return NotFound();

        return View(_mapper.Map<CategoryDto>(category));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = _unitOfWork.Categories.GetWithProducts(id);

        if (category == null)
            return NotFound();

        if (category.Products.Any())
        {
            ModelState.AddModelError(string.Empty,
                "Cannot delete this category because it has products assigned to it.");

            return View("Delete", _mapper.Map<CategoryDto>(category));
        }

        _unitOfWork.Categories.Delete(category);
        _unitOfWork.Complete();

        TempData["Success"] = "Category deleted successfully.";

        await _logger.LogAsync(
            _userManager.GetUserId(User),
            "DeleteCategory",
            $"Deleted: {category.Name} (ID: {category.Id})"
        );

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Search(string term)
    {
        var categories = _unitOfWork.Categories.GetAll();

        if (!string.IsNullOrWhiteSpace(term))
        {
            categories = categories
                .Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var mapped = _mapper.Map<IEnumerable<CategoryDto>>(categories);

        return PartialView("_CategoryTableRows", mapped);
    }




}
