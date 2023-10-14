using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Bulky.Models.ViewModels;
using Bulky.Utility;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Details(int orderId)
    {
        OrderVM orderVM = new()
        {
            OrderHeader = _unitOfWork.OrderHeaders.Get(u => u.Id == orderId, includeProperties:"ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetails.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
        };

        return View(orderVM);
    }

    #region API CALLS

    [HttpGet]
    public IActionResult GetAll(string status)
    {
        IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeaders.GetAll(includeProperties: "ApplicationUser").ToList();

        switch (status)
        {
            case "pending":
                objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                break;
            case "inprocess":
                objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusInProcess);
                break;
            case "completed":
                objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusShipped);
                break;
            case "approved":
                objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusApproved);
                break;
            default:
                break;
        }

        return Json(new { data = objOrderHeaders });
    }


    #endregion

}