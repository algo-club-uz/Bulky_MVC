using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
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

    [ActionName("Details")]
    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult Details_PAY_NOW()
    {
        OrderVM.OrderHeader =
            _unitOfWork.OrderHeaders.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
        OrderVM.OrderDetails = _unitOfWork.OrderDetails.GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id,
            includeProperties: "Product");

        //it is a regular customer account and we need to capture payment
        //stripe capture

        var domain = "https://localhost:7209/";


        var options = new SessionCreateOptions
        {
            SuccessUrl = domain + $"Admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
            CancelUrl = domain + $"Admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
        };

        foreach (var item in OrderVM.OrderDetails)
        {
            var sessionLineItem = new SessionLineItemOptions()
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Title
                    }
                },
                Quantity = item.Count
            };
            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);

        _unitOfWork.OrderHeaders.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
        _unitOfWork.Save();
        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeaders.Get(u => u.Id == orderHeaderId);

        if (orderHeader.PaymentStatus is  SD.PaymentStatusDelayedPayment)
        {
            //this is an order by company

            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);


            if (session.PaymentStatus.ToLower() is "paid")
            {
                _unitOfWork.OrderHeaders.UpdateStripePaymentId(orderHeaderId, session.Id,
                    session.PaymentIntentId);
                _unitOfWork.OrderHeaders.UpdateStatus(orderHeaderId, SD.StatusApproved, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }

        

        return View(orderHeaderId);
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



    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult StartProcessing()
    {
        _unitOfWork.OrderHeaders.UpdateStatus(OrderVM.OrderHeader.Id,SD.StatusInProcess);
        _unitOfWork.Save();

        TempData["Success"] = "Order Details Updated Successfully.";

        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult ShipOrder()
    {
        var orderHeader = _unitOfWork.OrderHeaders.Get(u => u.Id == OrderVM.OrderHeader.Id);

        orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
        orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
        orderHeader.OrderStatus = SD.StatusShipped;
        orderHeader.ShippingDate = DateTime.Now;

        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
        }

        _unitOfWork.OrderHeaders.Update(orderHeader);
        _unitOfWork.Save();

        TempData["Success"] = "Order Shipped Successfully.";

        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult CancelOrder()
    {
        var orderHeader = _unitOfWork.OrderHeaders.Get(u => u.Id == OrderVM.OrderHeader.Id);

        if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
        {
            var options = new RefundCreateOptions()
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderHeader.PaymentIntentId
            };

            var service = new RefundService();
            Refund refund = service.Create(options);

            _unitOfWork.OrderHeaders.UpdateStatus(orderHeader.Id,SD.StatusCancelled,SD.StatusRefunded);


        }
        else
        {
            _unitOfWork.OrderHeaders.UpdateStatus(orderHeader.Id,SD.StatusCancelled,SD.StatusCancelled);
        }

        _unitOfWork.Save();

        TempData["Success"] = "Order Cancelled Successfully.";

        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
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