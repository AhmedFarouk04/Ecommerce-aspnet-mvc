using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Cart? GetByUserId(string userId)
    {
        return _context.Carts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.UserId == userId);
    }

    public Cart GetOrCreate(string userId)
    {
        var cart = GetByUserId(userId);

        if (cart != null)
            return cart;

        cart = new Cart
        {
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Carts.Add(cart);
        return cart;
    }
}
