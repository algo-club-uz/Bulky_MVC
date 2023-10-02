using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;

namespace BulkyWeb.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unit;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unit)
    {
        _logger = logger;
        _unit = unit;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> products = _unit.Products.GetAll(includeProperties: "Category").ToList();
        return View(products);
    }
    public IActionResult Details(int id)
    {
        ShoppingCart cart = new()
        {
            Product = _unit.Products.Get(u => u.Id == id, "Category"),
            Count = 1,
            ProductId = id
    };
        return View(cart);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        shoppingCart.ApplicationUserId = userId;

        var cart = _unit.ShoppingCarts.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

        if (cart is not null)
        {
            //shopping cart exists 
            cart.Count += shoppingCart.Count;
            _unit.ShoppingCarts.Update(cart);
        }
        else
        {
            _unit.ShoppingCarts.Add(shoppingCart);
        }

        TempData["success"] = "Cart updated successfully";

        _unit.Save();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}