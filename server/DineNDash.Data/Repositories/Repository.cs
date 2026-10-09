using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DineNDash.Data.Repositories;

/// <summary>
/// One implementation of IRepository that works for every model.
/// </summary>
public class Repository<T>(AppDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await DbSet.AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id) =>
        await DbSet.FindAsync(id);

    public async Task<T> AddAsync(T entity)
    {
        entity.Validate();
        DbSet.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        entity.Validate();
        DbSet.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity is null) return;

        DbSet.Remove(entity);
        await context.SaveChangesAsync();
    }
}