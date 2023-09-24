using Bulky.Models;

namespace Bulky.DataAccess.Repositories.IRepositories;

public interface IProductRepository:IRepository<Product>
{
    void Update(Product product);
}