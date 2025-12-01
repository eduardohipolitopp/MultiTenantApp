using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<List<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
