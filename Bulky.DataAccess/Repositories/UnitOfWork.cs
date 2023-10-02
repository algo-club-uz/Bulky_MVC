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
    public IApplicationUserRepository ApplicationUsers { get; }
    public IOrderHeaderRepository OrderHeaders { get; }
    public IOrderDetailRepository OrderDetails { get; }

    public UnitOfWork(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        Products = new ProductRepository(_appDbContext);
        Categories = new CategoryRepository(_appDbContext);
        Companies = new CompanyRepository(_appDbContext);
        ShoppingCarts = new ShoppingCartRepository(_appDbContext);
        ApplicationUsers = new ApplicationUserRepository(_appDbContext);
        OrderDetails = new OrderDetailRepository(appDbContext);
        OrderHeaders = new OrderHeaderRepository(appDbContext);
    }

    public void Save()
    {
        _appDbContext.SaveChanges();
    }
}