using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
{
    private readonly AppDbContext _appDbContext;
    public ShoppingCartRepository(AppDbContext appDbContext) : base(appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public void Update(ShoppingCart shoppingCart)
    {
        _appDbContext.ShoppingCarts.Update(shoppingCart);
    }
}