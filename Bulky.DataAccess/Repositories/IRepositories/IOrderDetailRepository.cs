using Bulky.Models;

namespace Bulky.DataAccess.Repositories.IRepositories;

public interface IOrderDetailRepository : IRepository<OrderDetail>
{
    void Update(OrderDetail orderDetail);
    
}