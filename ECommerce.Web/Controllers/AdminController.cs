using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Identity;
using ECommerce.Web.Services;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AdminActivityLogger _logger;

        public AdminController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            AdminActivityLogger logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<IActionResult> Index(
    int page = 1,
    DateTime? startDate = null,
    DateTime? endDate = null,
    string search = "")
        {
            const int pageSize = 10;

            var productsQuery = _unitOfWork.Products.GetAll().AsQueryable();
            var categories = _unitOfWork.Categories.GetAll().ToList();
            var users = _userManager.Users.ToList();

            if (startDate.HasValue)
                productsQuery = productsQuery.Where(p => p.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                productsQuery = productsQuery.Where(p => p.CreatedAt <= endDate.Value);

            if (!string.IsNullOrWhiteSpace(search))
                productsQuery = productsQuery.Where(p => p.Name.Contains(search));

            var totalProducts = productsQuery.Count();

            var latestProducts = productsQuery
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int admins = 0, customers = 0;
            foreach (var u in users)
            {
                if (await _userManager.IsInRoleAsync(u, "Admin")) admins++;
                if (await _userManager.IsInRoleAsync(u, "Customer")) customers++;
            }

            var model = new AdminDashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalCategories = categories.Count,
                TotalUsers = users.Count,
                TotalAdmins = admins,
                TotalCustomers = customers,

                LatestProducts = latestProducts,

                ProductsPerCategory = categories.Select(c => new CategoryStatsViewModel
                {
                    CategoryId = c.Id,
                    Name = c.Name,
                    ProductsCount = productsQuery.Count(p => p.CategoryId == c.Id)
                }).ToList(),

                TotalStock = productsQuery.Sum(p => p.Stock),
                TotalStockValue = productsQuery.Sum(p => p.Stock * p.Price),

                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize),

                StartDate = startDate,
                EndDate = endDate,
                Search = search
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> FilterProducts(DateTime? startDate, DateTime? endDate, string search)
        {
            return RedirectToAction("Index", new { startDate, endDate, search });
        }







        public async Task<IActionResult> Users(
            string search,
            string role,
            string status,
            string emailStatus,
            string ordersFilter,
            string sort,
            int page = 1,
            int pageSize = 10)
        {
            var users = _userManager.Users.ToList();
            var orders = _unitOfWork.Orders.GetAll().ToList();

            var ordersByUser = orders
                .GroupBy(o => o.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            var usersList = new List<AdminUserViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                bool suspended = u.LockoutEnd != null && u.LockoutEnd > DateTime.UtcNow;

                usersList.Add(new AdminUserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    IsAdmin = roles.Contains("Admin"),
                    IsCustomer = roles.Contains("Customer"),
                    IsSuspended = suspended,
                    EmailConfirmed = u.EmailConfirmed,
                    RegistrationDate = u.RegistrationDate,
                    OrdersCount = ordersByUser.ContainsKey(u.Id) ? ordersByUser[u.Id] : 0,
                    ImageUrl = u.ImageUrl,
                    ThumbnailUrl = u.ThumbnailUrl
                });
            }

            if (!string.IsNullOrEmpty(search))
                usersList = usersList.Where(u =>
                    (u.UserName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();

            if (role == "Admin") usersList = usersList.Where(u => u.IsAdmin).ToList();
            if (role == "Customer") usersList = usersList.Where(u => u.IsCustomer).ToList();

            if (status == "active") usersList = usersList.Where(u => !u.IsSuspended).ToList();
            if (status == "suspended") usersList = usersList.Where(u => u.IsSuspended).ToList();

            if (emailStatus == "confirmed") usersList = usersList.Where(u => u.EmailConfirmed).ToList();
            if (emailStatus == "unconfirmed") usersList = usersList.Where(u => !u.EmailConfirmed).ToList();

            if (ordersFilter == "withOrders") usersList = usersList.Where(u => u.OrdersCount > 0).ToList();
            if (ordersFilter == "withoutOrders") usersList = usersList.Where(u => u.OrdersCount == 0).ToList();

            usersList = sort switch
            {
                "name_desc" => usersList.OrderByDescending(u => u.UserName).ToList(),
                "email_asc" => usersList.OrderBy(u => u.Email).ToList(),
                "email_desc" => usersList.OrderByDescending(u => u.Email).ToList(),
                "date_newest" => usersList.OrderByDescending(u => u.RegistrationDate).ToList(),
                "date_oldest" => usersList.OrderBy(u => u.RegistrationDate).ToList(),
                "orders_high" => usersList.OrderByDescending(u => u.OrdersCount).ToList(),
                "orders_low" => usersList.OrderBy(u => u.OrdersCount).ToList(),
                _ => usersList.OrderBy(u => u.UserName).ToList()
            };


            int totalUsers = usersList.Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalUsers / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var pageData = usersList.Skip((page - 1) * pageSize).Take(pageSize).ToList();


            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRole = role;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentEmailStatus = emailStatus;
            ViewBag.CurrentOrdersFilter = ordersFilter;
            ViewBag.CurrentSort = sort;

            return View(pageData);
        }


        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return View(new AdminUserDetailsViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                ThumbnailUrl = user.ThumbnailUrl,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                RegistrationDate = user.RegistrationDate,
                IsAdmin = roles.Contains("Admin"),
                IsCustomer = roles.Contains("Customer"),
                IsSuspended = user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow,
                Orders = _unitOfWork.Orders.GetOrdersForUser(user.Id),
                Reviews = _unitOfWork.Reviews.GetUserReviews(user.Id),
                Ratings = _unitOfWork.Ratings.GetAll().Where(r => r.UserId == user.Id),
                Wishlist = _unitOfWork.Wishlists.GetUserWishlist(user.Id)
            });
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.AddToRoleAsync(user, "Admin");

            await _logger.LogAsync(
                _userManager.GetUserId(User),
                "MakeAdmin",
                $"Promoted: {user.UserName}");

            return RedirectToAction(nameof(Users));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
                return RedirectToAction(nameof(Users));

            await _userManager.RemoveFromRoleAsync(user, "Admin");

            await _logger.LogAsync(
                _userManager.GetUserId(User),
                "RemoveAdmin",
                $"Removed: {user.UserName}");

            return RedirectToAction(nameof(Users));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot delete yourself.";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.DeleteAsync(user);

            await _logger.LogAsync(
                _userManager.GetUserId(User),
                "DeleteUser",
                $"Deleted: {user.UserName}");

            return RedirectToAction(nameof(Users));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddYears(20);

            await _userManager.UpdateAsync(user);

            await _logger.LogAsync(
                _userManager.GetUserId(User),
                "SuspendUser",
                $"Suspended: {user.UserName}");

            return RedirectToAction(nameof(Users));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnabled = false;
            user.LockoutEnd = null;

            await _userManager.UpdateAsync(user);

            await _logger.LogAsync(
                _userManager.GetUserId(User),
                "RestoreUser",
                $"Restored: {user.UserName}");

            return RedirectToAction(nameof(Users));
        }


        public IActionResult ActivityLogs(
     string search,
     string admin,
     string actionName,
     int page = 1,
     int pageSize = 15)
        {
            var allLogs = _unitOfWork.AdminActivityLogs.GetAll()
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            var logs = allLogs;

            if (!string.IsNullOrEmpty(search))
                logs = logs.Where(x =>
                    (x.Action?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Target?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();

            if (!string.IsNullOrEmpty(admin))
                logs = logs.Where(x => x.AdminId == admin).ToList();

            if (!string.IsNullOrWhiteSpace(actionName))
            {
                logs = logs.Where(x =>
                    x.Action != null &&
                    x.Action.Equals(actionName, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }


            int totalItems = logs.Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var pageData = logs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.UniqueAdmins = allLogs.Select(x => x.AdminId).Distinct().ToList();
            ViewBag.UniqueActions = allLogs.Select(x => x.Action).Distinct().ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            ViewBag.Search = search;
            ViewBag.Admin = admin;
            ViewBag.ActionName = actionName;
            foreach (var l in allLogs)
            {
                Console.WriteLine($"[{l.Action}]");
            }


            return View(pageData);
        }

    }
}
