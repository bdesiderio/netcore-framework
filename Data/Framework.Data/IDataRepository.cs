using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Data
{
    public interface IDataRepository<TEntity, TKey> where TEntity : class, new()
    {
        Task<TEntity> GetAsync(TKey key);
        Task<TEntity> TryGetAsync(TKey key);
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        Task UpdateAsync(TEntity entity);
        Task RemoveAsync(TKey key);
        Task<IList<TEntity>> GetAllAsync();
        Task<Paginable<TEntity>> GetAllAsync(int currentPage, int pageSize);
        Task UpdateRangeAsync(IEnumerable<TEntity> entities);
        Task RemoveAsync(TEntity entity);
        Task RemoveRangeAsync(IEnumerable<TEntity> entity);
    }

    public interface IUpdateBuilder<TEntity> where TEntity : class
    {
        IUpdateBuilder<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value);
    }
}
