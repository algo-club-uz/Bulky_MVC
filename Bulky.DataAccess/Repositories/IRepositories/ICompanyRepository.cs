using Bulky.Models;

namespace Bulky.DataAccess.Repositories.IRepositories;

public interface ICompanyRepository:IRepository<Company>
{
    void Update(Company company);
}