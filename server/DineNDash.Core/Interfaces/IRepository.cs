using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;

/// <summary>
/// Backed by EF Core in the API and an in-memory list in tests
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    /// <summary>
    /// Does nothing when the id does not exist, so callers need not check first
    /// </summary>
    Task DeleteAsync(int id);
}