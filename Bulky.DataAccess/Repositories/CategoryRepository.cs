using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class CategoryRepository: Repository<Category>,ICategoryRepository
{
    private readonly AppDbContext _appDbContext;
    public CategoryRepository(AppDbContext appDbContext) : base(appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Update(Category category)
    {
        _appDbContext.Categories.Update(category);
        _appDbContext.SaveChanges();
    }

    public void Save()
    {
        _appDbContext.SaveChanges();
    }
}