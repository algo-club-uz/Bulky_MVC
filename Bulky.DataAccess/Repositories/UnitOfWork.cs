using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.IRepositories;

namespace Bulky.DataAccess.Repositories;

public class UnitOfWork:IUnitOfWork
{
    private readonly AppDbContext _appDbContext;
    public ICategoryRepository Categories { get; }
    public IProductRepository Products { get; }
    public ICompanyRepository Companies { get; }
    public IShoppingCartRepository ShoppingCarts { get; }

    public UnitOfWork(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        Products = new ProductRepository(_appDbContext);
        Categories = new CategoryRepository(_appDbContext);
        Companies = new CompanyRepository(_appDbContext);
        ShoppingCarts = new ShoppingCartRepository(_appDbContext);
    }

    public void Save()
    {
        _appDbContext.SaveChanges();
    }
}