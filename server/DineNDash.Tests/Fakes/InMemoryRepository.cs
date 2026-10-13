using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Tests.Fakes;

/// <summary>
/// In-memory IRepository used by tests instead of PostgreSQL.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly List<T> _items = [];
    private int _nextId = 1;

    public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult<IEnumerable<T>>(_items.ToList());

    public Task<T?> GetByIdAsync(int id) => Task.FromResult(_items.FirstOrDefault(i => i.Id == id));

    public Task<T> AddAsync(T entity)
    {
        entity.Validate();
        entity.Id = _nextId++;
        _items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity)
    {
        entity.Validate();
        _items.RemoveAll(i => i.Id == entity.Id);
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        _items.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }
}
