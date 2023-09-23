using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.IRepositories;

namespace Bulky.DataAccess.Repositories;

public class UnitOfWork:IUnitOfWork
{
    private readonly AppDbContext _appDbContext;
    public ICategoryRepository Categories { get; }

    public UnitOfWork(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        Categories = new CategoryRepository(_appDbContext);
    }

    public void Save()
    {
        _appDbContext.SaveChanges();
    }
}