using System.Linq.Expressions;

namespace ECommerce.Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        IQueryable<T> Query();
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
