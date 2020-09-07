using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Data
{
    public interface IDataRepository<TDomainEntity, TKey>
    {
        Task<IList<TDomainEntity>> GetAllAsync();
        Task<IList<TDomainEntity>> GetAllAsync(int page, int pageSize);
        Task<TDomainEntity> GetByIdAsync(TKey id);
        Task<TDomainEntity> CreateAsync(TDomainEntity entity);
        Task<TDomainEntity> UpdateAsync(TDomainEntity entity);
        Task DeleteAsync(TKey entity);
    }
}
