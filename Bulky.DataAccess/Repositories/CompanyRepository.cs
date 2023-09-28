using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class CompanyRepository:Repository<Company>, ICompanyRepository
{
    private readonly AppDbContext _appDbContext;

    public CompanyRepository(AppDbContext appDbContext):base(appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Update(Company company)
    {
        _appDbContext.Companies.Update(company);
        _appDbContext.SaveChanges();
    }
}