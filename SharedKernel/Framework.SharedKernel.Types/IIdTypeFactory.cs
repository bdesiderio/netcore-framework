using System;
using System.Threading.Tasks;

namespace Framework.SharedKernel.Types
{
    public interface IIdTypeFactory
    {
        Task<object> GetIdType<TSource>(TSource source);
    }
}
