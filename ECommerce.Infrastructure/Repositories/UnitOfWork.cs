using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;

    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public IOrderRepository Orders { get; }
    public IWishlistRepository Wishlists { get; }
    public IRatingRepository Ratings { get; }
    public IReviewRepository Reviews { get; }
    public ICartRepository Carts { get; }

    
    public IAdminActivityLogRepository AdminActivityLogs { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        Products = new ProductRepository(_context);
        Categories = new CategoryRepository(_context);
        Orders = new OrderRepository(_context);
        Wishlists = new WishlistRepository(_context);
        Ratings = new RatingRepository(_context);
        Reviews = new ReviewRepository(_context);

        AdminActivityLogs = new AdminActivityLogRepository(_context);
        Carts = new CartRepository(_context);
    }


    public void BeginTransaction()
    {
        _transaction = _context.Database.BeginTransaction();
    }

    public void Commit()
    {
        _transaction?.Commit();
    }

    public void Rollback()
    {
        _transaction?.Rollback();
    }

    public int Complete()
    {
        return _context.SaveChanges();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
