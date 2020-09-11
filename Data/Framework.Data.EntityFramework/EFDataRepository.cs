using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Data.EntityFramework
{
    public abstract class EFDataRepository<TDbEntity, TDomainEntity, TKey, TDbContext> : IDataRepository<TDomainEntity, TKey>
        where TDbEntity : class
        where TDbContext : DbContext
    {
        protected readonly TDbContext DbContext;

        public EFDataRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public abstract Func<TDomainEntity, TKey> KeyDomainField { get; }
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

        public virtual async Task<TDomainEntity> GetAsync(TKey id)
        {
            var bdEntity = await GetQueryable().FirstOrDefaultAsync(GetByIdSelector(id));
            return await MapToDomainEntity(bdEntity);
        }

        public abstract Task<TDbEntity> CreateDbEntityAsync();

        public abstract Task<TDomainEntity> MapToDomainEntity(TDbEntity dbEntity);

        public async Task<IList<TDomainEntity>> MapToDomainEntity(IList<TDbEntity> dbEntities)
        => (await Task.WhenAll(dbEntities
            .Select(ev => MapToDomainEntity(ev))))
            .Where(result => result != null).ToList();

        public abstract Task<TDbEntity> MapFromDomainEntity(TDbEntity dbEntity, TDomainEntity domainEntity);


        protected virtual IQueryable<TDbEntity> GetQueryable() => Entities;

        protected IQueryable<TDbEntity> Paginate(IQueryable<TDbEntity> tDbEntities, int page, int pageSize)
            => tDbEntities.Skip(pageSize * (page - 1)).Take(pageSize);

        public virtual async Task<IList<TDomainEntity>> GetAllAsync(int page, int pageSize)
        {
            return await MapToDomainEntity(await Paginate(GetQueryable(), page, pageSize).ToListAsync());
        }

        public virtual async Task<IList<TDomainEntity>> GetAllAsync()
        {
            return await MapToDomainEntity(await GetQueryable().ToListAsync());
        }

        public virtual async Task<TDomainEntity> GetByIdAsync(TKey id)
        {
            var bdEntity = await GetQueryable().FirstOrDefaultAsync(GetByIdSelector(id));

            if (bdEntity == null)
            {
                return default(TDomainEntity);
            }

            return await MapToDomainEntity(bdEntity);
        }

        public virtual async Task<TDomainEntity> CreateAsync(TDomainEntity domainEntity)
        {
            var dbEntity = await CreateDbEntityAsync();

            dbEntity = await MapFromDomainEntity(dbEntity, domainEntity);

            var createdEntity = await Entities.AddAsync(dbEntity);

            await DbContext.SaveChangesAsync();

            return await MapToDomainEntity(createdEntity.Entity);
        }

        public virtual async Task<TDomainEntity> UpdateAsync(TDomainEntity domainEntity)
        {
            var domainEntityId = KeyDomainField(domainEntity);
            var dbEntity = await GetQueryable().FirstOrDefaultAsync(GetByIdSelector(domainEntityId));

            dbEntity = await MapFromDomainEntity(dbEntity, domainEntity);
            var updatedEntity = Entities.Update(dbEntity);

            await DbContext.SaveChangesAsync();

            return await MapToDomainEntity(updatedEntity.Entity);
        }

        public virtual async Task DeleteAsync(TKey entityId)
        {
            var dbEntity = await GetQueryable().FirstOrDefaultAsync(GetByIdSelector(entityId));

            Entities.Remove(dbEntity);

            await DbContext.SaveChangesAsync();
        }
    }
}
