using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Data.EntityFramework
{
    public abstract class EFDataRepository<TDbEntity, TDomainEntity, TKey, TDbContext> : IDataRepository<TDomainEntity, Guid>
        where TDbEntity : class
        where TDbContext : DbContext
    {
        protected readonly TDbContext DbContext;

        public EFDataRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public abstract Func<TDomainEntity, TKey> KeyDomainField { get; }
        public abstract Func<TDbEntity, TKey> KeyDbField { get; }

        public abstract DbSet<TDbEntity> Entities { get; }

        public virtual async Task<TDomainEntity> GetAsync(TKey id)
        {
            var bdEntity = await GetQueryable().FirstOrDefaultAsync(x => KeyDbField(x).Equals(id));

            return await MapToDomainEntity(bdEntity);
        }

        public abstract Task<TDomainEntity> MapToDomainEntity(TDbEntity dbEntity);
        public abstract Task<IList<TDomainEntity>> MapToDomainEntity(IList<TDbEntity> dbEntity);

        public abstract Task<TDbEntity> MapFromDomainEntity(TDomainEntity dbEntity);


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

        public virtual async Task<TDomainEntity> GetByIdAsync(Guid id)
        {
            var entity = await GetQueryable().FirstOrDefaultAsync(x => KeyDbField(x).Equals(id));

            if (entity == null)
            {
                return default(TDomainEntity);
            }

            return await MapToDomainEntity(entity);
        }

        public virtual async Task<TDomainEntity> CreateAsync(TDomainEntity domainEntity)
        {
            var bdEntity = await MapFromDomainEntity(domainEntity);

            var createdEntity = await Entities.AddAsync(bdEntity);

            await DbContext.SaveChangesAsync();

            return await MapToDomainEntity(createdEntity.Entity);
        }

        public virtual async Task<TDomainEntity> UpdateAsync(TDomainEntity domainEntity)
        {
            var bdEntity = await Entities.FirstOrDefaultAsync(x => KeyDbField(x).Equals(KeyDomainField(domainEntity)));

            var updatedEntity = Entities.Update(bdEntity);

            await DbContext.SaveChangesAsync();

            return await MapToDomainEntity(updatedEntity.Entity);
        }

        public virtual async Task DeleteAsync(Guid entityId)
        {
            var bdEntity = Entities.FirstOrDefault(x => KeyDbField(x).Equals(entityId));

            Entities.Remove(bdEntity);

            await DbContext.SaveChangesAsync();
        }
    }
}
