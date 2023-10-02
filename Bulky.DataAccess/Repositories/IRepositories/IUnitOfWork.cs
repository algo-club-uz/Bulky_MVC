namespace Bulky.DataAccess.Repositories.IRepositories;

public interface IUnitOfWork
{
    ICategoryRepository Categories { get; }

    IProductRepository Products { get; }

    ICompanyRepository Companies { get; }

    IShoppingCartRepository ShoppingCarts { get; }

    void Save();
}