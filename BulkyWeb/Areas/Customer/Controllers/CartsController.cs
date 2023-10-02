using Bulky.DataAccess.Repositories;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BulkyWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartsController : Controller
{
    private readonly UnitOfWork _unitOfWork;
    public ShoppingCartVM ShoppingCartVM { get; set; }
    public CartsController(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVM = new()
        {
            ShoppingCartList = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == userId, includeProperties:"Product" )
        };

        return View(ShoppingCartVM);
    }
}