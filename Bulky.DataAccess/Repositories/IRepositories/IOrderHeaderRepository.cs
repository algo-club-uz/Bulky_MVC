using Bulky.Models;

namespace Bulky.DataAccess.Repositories.IRepositories;

public interface IOrderHeaderRepository : IRepository<OrderHeader>
{
    void Update(OrderHeader orderHeader);
    
}