using Bulky.Models;

namespace Bulky.DataAccess.Repositories.IRepositories;

public interface ICategoryRepository:IRepository<Category>
{
    void Update(Category category);

    void Save();
}