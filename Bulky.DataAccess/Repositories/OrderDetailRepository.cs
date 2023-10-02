using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
{
    private readonly AppDbContext _appDbContext;
    public OrderDetailRepository(AppDbContext appDbContext) : base(appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Update(OrderDetail orderDetail)
    {
        _appDbContext.OrderDetails.Update(orderDetail);
        _appDbContext.SaveChanges();
    }
    
}