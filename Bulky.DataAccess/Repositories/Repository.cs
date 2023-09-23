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

    public T Get(Expression<Func<T, bool>> filter)
    {
        IQueryable<T> query = dbSet;
        query = query.Where(filter);

        return query.FirstOrDefault()!;
    }

    public IEnumerable<T> GetAll()
    {
        IQueryable<T> query = dbSet;

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