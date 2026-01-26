using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace MultiTenantApp.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        private static readonly bool SupportsLogicalDelete = typeof(T).IsDefined(typeof(LogicalDeleteAttribute), false);

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            var query = _dbSet.AsQueryable();
            
            // Apply soft delete filter if entity supports it
            if (SupportsLogicalDelete)
            {
                query = query.Where(e => !e.IsDeleted);
            }
            
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<T>> GetAllAsync()
        {
            var query = _dbSet.AsQueryable();
            
            // Apply soft delete filter if entity supports it
            if (SupportsLogicalDelete)
            {
                query = query.Where(e => !e.IsDeleted);
            }
            
            return await query.ToListAsync();
        }

        public async Task<(List<T> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null)
        {
            var query = _dbSet.AsQueryable();

            // Apply soft delete filter if entity supports it
            if (SupportsLogicalDelete)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.AsQueryable();
            
            // Apply soft delete filter if entity supports it
            if (SupportsLogicalDelete)
            {
                query = query.Where(e => !e.IsDeleted);
            }
            
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<T> AddAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(T entity)
        {
            if (SupportsLogicalDelete)
            {
                // Soft delete
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            else
            {
                // Hard delete
                _dbSet.Remove(entity);
            }
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Restores a soft-deleted entity. Only works if the entity supports logical delete.
        /// </summary>
        public async Task RestoreAsync(T entity)
        {
            if (!SupportsLogicalDelete)
            {
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not support logical delete.");
            }

            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Permanently deletes an entity, even if it supports logical delete.
        /// </summary>
        public async Task HardDeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }
    }
}
