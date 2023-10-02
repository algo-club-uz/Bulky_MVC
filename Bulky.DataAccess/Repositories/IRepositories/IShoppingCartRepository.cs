using Bulky.Models;

namespace Bulky.DataAccess.Repositories.IRepositories;

public interface IShoppingCartRepository : IRepository<ShoppingCart>
{
    void Update(ShoppingCart shoppingCart);
}