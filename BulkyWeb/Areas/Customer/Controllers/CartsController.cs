using Bulky.DataAccess.Repositories;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;

namespace BulkyWeb.Areas.Customer.Controllers;

[Area("customer")]
[Authorize]
public class CartsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
    public ShoppingCartVM ShoppingCartVM { get; set; }
    public CartsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVM = new()
        {
            ShoppingCartList = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == userId, includeProperties:"Product" ),
            OrderHeader = new OrderHeader()

        };

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVM);
    }

    public IActionResult Plus(int cartId)
    {
        var cartFromDb = _unitOfWork.ShoppingCarts.Get(u => u.Id == cartId);
        cartFromDb.Count += 1;
        _unitOfWork.ShoppingCarts.Update(cartFromDb);

        return RedirectToAction(nameof(Index));
    }
    public IActionResult Minus(int cartId)
    {
        var cartFromDb = _unitOfWork.ShoppingCarts.Get(u => u.Id == cartId);
        if (cartFromDb.Count <= 1)
        {
            //remove that from cart
            _unitOfWork.ShoppingCarts.Remove(cartFromDb);
            _unitOfWork.Save();
        }
        else
        {
            cartFromDb.Count -= 1;
            _unitOfWork.ShoppingCarts.Update(cartFromDb);
        }

        return RedirectToAction(nameof(Index));
    }
    public IActionResult Remove(int cartId)
    {
        var cartFromDb = _unitOfWork.ShoppingCarts.Get(u => u.Id == cartId);
            //remove that from cart
            _unitOfWork.ShoppingCarts.Remove(cartFromDb);
            _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVM = new()
        {
            ShoppingCartList = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
            OrderHeader = new OrderHeader()

        };

        var user = _unitOfWork.ApplicationUsers.Get(u => u.Id == userId);
        ShoppingCartVM.OrderHeader.ApplicationUser = user;

        ShoppingCartVM.OrderHeader.Name = user.Name;
        ShoppingCartVM.OrderHeader.PhoneNumber = user.PhoneNumber;
        ShoppingCartVM.OrderHeader.StreetAddress = user.StreetAddress;
        ShoppingCartVM.OrderHeader.City = user.City;
        ShoppingCartVM.OrderHeader.State = user.State;
        ShoppingCartVM.OrderHeader.PostalCode = user.PostalCode;

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }
        return View(ShoppingCartVM);
    }

    [HttpPost]
    [ActionName("SummaryPOST")]
    public IActionResult SummaryPOST()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVM.ShoppingCartList =
            _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

        ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
        ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

        var user = _unitOfWork.ApplicationUsers.Get(u => u.Id == userId);
        ShoppingCartVM.OrderHeader.ApplicationUser = user;
        

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        var check = user.CompanyId.GetValueOrDefault() == 0;

        if (check)
        {
            //it is a regular customer account and we need to capture payment
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
        }
        else
        {
            //it is a company
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        }

        _unitOfWork.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
        _unitOfWork.Save();

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            OrderDetail orderDetail = new()
            {
                ProductId =  cart.ProductId,
                OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count
            };
            _unitOfWork.OrderDetails.Add(orderDetail);
            _unitOfWork.Save();
        }

        return RedirectToAction();
    }

    private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
    {
        if (shoppingCart.Count <= 50)
        {
            return shoppingCart.Product.Price;
        }
        else
        {
            if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}