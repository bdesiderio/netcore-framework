using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Data.EntityFramework
{
    public abstract class EFDataRepository<TDbEntity, TKey, TDbContext> : IDataRepository<TDbEntity, TKey>
        where TDbEntity : class
        where TDbContext : DbContext
    {
        protected readonly TDbContext DbContext;

        public EFDataRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public abstract Expression<Func<TDbEntity, TKey>> KeyDbField { get; }

        public abstract DbSet<TDbEntity> Entities { get; }

        private Expression<Func<TDbEntity, bool>> GetByIdSelector(TKey id)
        {
            var keySelector = KeyDbField;
            return Expression.Lambda<Func<TDbEntity, bool>>(
                Expression.Equal(
                    keySelector.Body,
                    Expression.Constant(id)
                ),
                keySelector.Parameters.Single()
            );
        }

        public virtual async Task<TDbEntity> GetAsync(TKey id)
        {
            return await GetQueryable().FirstOrDefaultAsync(GetByIdSelector(id));
        }

        protected virtual IQueryable<TDbEntity> GetQueryable() => Entities;

        protected virtual async Task OnCreating(TDbEntity domainEntity) => await Task.CompletedTask;

        protected IQueryable<TDbEntity> Paginate(IQueryable<TDbEntity> tDbEntities, int page, int pageSize)
            => tDbEntities.Skip(pageSize * (page - 1)).Take(pageSize);

        public virtual async Task<IList<TDbEntity>> GetAllAsync(int page, int pageSize)
        {
            return await Paginate(GetQueryable(), page, pageSize).ToListAsync();
        }

        public virtual async Task<IList<TDbEntity>> GetAllAsync()
        {
            return await GetQueryable().ToListAsync();
        }

        public virtual async Task<TDbEntity> GetByIdAsync(TKey id)
        {
            var bdEntity = await GetQueryable().FirstOrDefaultAsync(GetByIdSelector(id));

            if (bdEntity == null)
            {
                return default(TDbEntity);
            }

            return bdEntity;
        }

        public virtual async Task<TDbEntity> CreateAsync(TDbEntity domainEntity)
        {
            var createdEntity = await Entities.AddAsync(domainEntity);

            await OnCreating(domainEntity);

            await DbContext.SaveChangesAsync();

            return createdEntity.Entity;
        }

        public virtual async Task<TDbEntity> UpdateAsync(TDbEntity domainEntity)
        {
            var updatedEntity = Entities.Update(domainEntity);

            DbContext.Entry(domainEntity).State = EntityState.Modified;

            await DbContext.SaveChangesAsync();

            return updatedEntity.Entity;
        }

        public virtual async Task DeleteAsync(TKey entityId)
        {
            var dbEntity = await GetQueryable().FirstOrDefaultAsync(GetByIdSelector(entityId));

            Entities.Remove(dbEntity);

            await DbContext.SaveChangesAsync();
        }
    }
}
