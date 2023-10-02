using Bulky.DataAccess.Repositories.IRepositories;
using System.Linq.Expressions;
using Bulky.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _appDbContext;
    internal DbSet<T> dbSet;
    public Repository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        dbSet = _appDbContext.Set<T>();
    }

    public void Add(T entity)
    {
        dbSet.Add(entity);
        
    }

    public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
    {
        IQueryable<T> query;
        query = tracked ? dbSet : dbSet.AsNoTracking();
        query = query.Where(filter);
        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProp in includeProperties
                         .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }
        return query.FirstOrDefault()!;
    }

    public IEnumerable<T> GetAll(string? includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProp in includeProperties
                         .Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }

        return query.ToList();
    }

    public void Remove(T entity)
    {
        dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        dbSet.RemoveRange(entities);
    }
}