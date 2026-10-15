using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DineNDash.Data.Repositories;

/// <summary>
/// One implementation of IRepository that works for every model
/// </summary>
public class Repository<T>(AppDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    /// <summary>
    /// Returns every row untracked, so callers cannot accidentally save changes
    /// </summary>
    public async Task<IEnumerable<T>> GetAllAsync() =>
        await DbSet.AsNoTracking().ToListAsync();

    /// <summary>
    /// Returns one row by id, or null when it does not exist
    /// </summary>
    public async Task<T?> GetByIdAsync(int id) =>
        await DbSet.FindAsync(id);

    /// <summary>
    /// Validates then inserts the entity and returns it with its new id
    /// </summary>
    public async Task<T> AddAsync(T entity)
    {
        entity.Validate();
        DbSet.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Validates then saves every column of the entity
    /// </summary>
    public async Task UpdateAsync(T entity)
    {
        entity.Validate();
        DbSet.Update(entity);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes the row with that id and does nothing if it is already gone
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity is null) return;

        DbSet.Remove(entity);
        await context.SaveChangesAsync();
    }
}