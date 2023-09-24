namespace Bulky.DataAccess.Repositories.IRepositories;

public interface IUnitOfWork
{
    ICategoryRepository Categories { get; }

    IProductRepository Products { get; }

    void Save();
}