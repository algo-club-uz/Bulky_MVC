using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    [BindProperty]
    public OrderVM OrderVM { get; set; }

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
        OrderVM  = new()
        {
            OrderHeader = _unitOfWork.OrderHeaders.Get(u => u.Id == orderId, includeProperties:"ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetails.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
        };

        return View(OrderVM);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult UpdateOrderDetail()
    {
        var orderFromDb = _unitOfWork.OrderHeaders.Get(u => u.Id == OrderVM.OrderHeader.Id);

        orderFromDb.Name = OrderVM.OrderHeader.Name;
        orderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
        orderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
        orderFromDb.City = OrderVM.OrderHeader.City;
        orderFromDb.State = OrderVM.OrderHeader.State;
        orderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

        if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
        {
            orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
        }

        _unitOfWork.OrderHeaders.Update(orderFromDb);
        _unitOfWork.Save();

        TempData["Success"] = "Order Details Updated Successfully.";

        return RedirectToAction(nameof(Details), new { orderId = orderFromDb.Id });

    }


    #region API CALLS

    [HttpGet]
    public IActionResult GetAll(string status)
    {
        IEnumerable<OrderHeader> objOrderHeaders;

        if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
        {
            objOrderHeaders = _unitOfWork.OrderHeaders.GetAll(includeProperties: "ApplicationUser").ToList();
        }
        else
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            objOrderHeaders = _unitOfWork.OrderHeaders
                .GetAll(u => u.ApplicationUserId == userId,includeProperties: "ApplicationUser");
        }
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