using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
{
    private readonly AppDbContext _appDbContext;
    public OrderHeaderRepository(AppDbContext appDbContext) : base(appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Update(OrderHeader orderHeader)
    {
        _appDbContext.OrderHeaders.Update(orderHeader);
        _appDbContext.SaveChanges();
    }
    
}