using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Framework.Data.EntityFramework
{
    public abstract class EFDataRepository<TEntity, TKey, TDbContext> : IDataRepository<TEntity, TKey>
        where TEntity : class, new()
        where TDbContext : DbContext
    {
        private readonly Action<TEntity, TKey> _keySetter;

        public EFDataRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
            _keySetter = KeyProperty.GetSetter();
        }

        public abstract Expression<Func<TEntity, TKey>> KeyProperty { get; }

        protected TDbContext DbContext { get; }

        protected virtual IQueryable<TEntity> GetAsyncQueryable() => DbContext.Set<TEntity>();

        public virtual async Task<TEntity> GetAsync(TKey key)
        {
            var entity = await GetAsyncQueryable().SingleAsync(KeyProperty.GetEqualsPredicateForConstant(key));
            //DbContext.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        public virtual async Task<TEntity> TryGetAsync(TKey key)
        {
            var entity = await DbContext.Set<TEntity>().SingleOrDefaultAsync(KeyProperty.GetEqualsPredicateForConstant(key));
            //DbContext.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        public virtual async Task<IList<TEntity>> GetAllAsync()
        {
            return await DbContext.Set<TEntity>().ToListAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            OnAdding(entity);
            DbContext.Set<TEntity>().Add(entity);
            await DbContext.SaveChangesAsync();
            DbContext.Entry(entity).State = EntityState.Detached;
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            DbContext.Set<TEntity>().Attach(entity);
            OnUpdating(entity);
            DbContext.Set<TEntity>().Update(entity);
            await SaveChangesAndDetachAsync(entity);
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            //DbContext.Set<TEntity>().AttachRange(entities);
            foreach (var entity in entities)
            {
                OnUpdating(entity);
            }
            DbContext.Set<TEntity>().UpdateRange(entities);
            await SaveChangesAndDetachAsync(entities);
        }

        public virtual Task RemoveAsync(TKey key)
        {
            var entity = new TEntity();
            _keySetter(entity, key);
            return RemoveAsync(entity);
        }

        public virtual async Task RemoveAsync(TEntity entity)
        {
            DbContext.Set<TEntity>().Remove(entity);
            await SaveChangesAndDetachAsync(entity);
        }

        public async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            DbContext.Set<TEntity>().RemoveRange(entities);
            await SaveChangesAndDetachAsync(entities);
        }

        protected virtual async Task SaveChangesAndDetachAsync(TEntity entity)
        {
            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw UpdateConcurrencyException.From(ex);
            }
            finally
            {
                DbContext.Entry(entity).State = EntityState.Detached;
            }
        }

        protected virtual async Task SaveChangesAndDetachAsync(IEnumerable<TEntity> entities)
        {
            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw UpdateConcurrencyException.From(ex);
            }
            finally
            {
                foreach (var entity in entities)
                {
                    DbContext.Entry(entity).State = EntityState.Detached;
                }
            }
        }

        protected virtual async Task SaveUpdateChangesAsync(TEntity entity)
        {
            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw UpdateConcurrencyException.From(ex);
            }

            DbContext.Entry(entity).State = EntityState.Detached;
        }

        protected virtual void OnAdding(TEntity entity) { }
        protected virtual void OnUpdating(TEntity entity) { }
        protected virtual void OnUpdating(TEntity entity, EntityEntry<TEntity> entityEntry) { }
    }

    public static class LinqExtensions
    {
        public static Expression<Func<TClass, bool>> GetEqualsPredicateForConstant<TClass, TProperty>(
            this Expression<Func<TClass, TProperty>> propertyAccessorExpression,
            TProperty value)
        {
            var predicateExpression = Expression.Equal(propertyAccessorExpression.Body, Expression.Constant(value));
            return Expression.Lambda<Func<TClass, bool>>(predicateExpression, propertyAccessorExpression.Parameters[0]);
        }

        public static Action<TClass, TProperty> GetSetter<TClass, TProperty>(
           this Expression<Func<TClass, TProperty>> propertyAccessorExpression)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertyAccessorExpression.Body).Member;
            return (Action<TClass, TProperty>)Delegate.CreateDelegate(typeof(Action<TClass, TProperty>), propertyInfo.GetSetMethod());
        }
    }

    public class EFCoreUpdateBuilder<TEntity> : IUpdateBuilder<TEntity> where TEntity : class
    {
        private readonly TEntity _entity;
        private readonly EntityEntry<TEntity> _entityEntry;

        public EFCoreUpdateBuilder(TEntity entity, EntityEntry<TEntity> entityEntry)
        {
            _entity = entity;
            _entityEntry = entityEntry;
        }

        public IUpdateBuilder<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
        {
            propertyExpression.GetSetter()(_entity, value);
            _entityEntry.Property(propertyExpression).IsModified = true;
            return this;
        }
    }

    public class UpdateConcurrencyException : Exception
    {
        public UpdateConcurrencyException() { }

        public UpdateConcurrencyException(string message) : base(message) { }

        public UpdateConcurrencyException(string message, Exception innerException) : base(message, innerException) { }

        public static UpdateConcurrencyException From(Exception ex)
            => new UpdateConcurrencyException(ex.Message, ex.InnerException);
    }
}
