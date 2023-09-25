
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductsController : Controller
{
    private readonly IUnitOfWork _unit;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unit = unitOfWork;
    }

    public IActionResult Index()
    {
        List<Product> products = _unit.Products.GetAll().ToList();
        IEnumerable<SelectListItem> CategoryList = _unit.Categories.GetAll().Select(u => new SelectListItem
        {
            Text = u.Name,
            Value = u.Id.ToString()
        });
        return View(products);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _unit.Products.Add(product);
            _unit.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }
        return View();
    }

    public IActionResult Edit(int? id)
    {
        if (id == null && id == 0)
        {
            return NotFound();
        }

        var product = _unit.Products.Get(u => u.Id == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost]
    public IActionResult Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            _unit.Products.Update(product);
            TempData["success"] = "Product updated successfully";
            return RedirectToAction("Index", "Products");
        }
        return View();
    }
    public IActionResult Delete(int? id)
    {
        if (id == null && id == 0)
        {
            return NotFound();
        }

        var product = _unit.Products.Get(u => u.Id == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeletePost(int? id)
    {
        var product = _unit.Products.Get(u => u.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        _unit.Products.Remove(product);
        _unit.Save();
        TempData["success"] = "Product deleted successfully";
        return RedirectToAction("Index", "Products");
    }


}