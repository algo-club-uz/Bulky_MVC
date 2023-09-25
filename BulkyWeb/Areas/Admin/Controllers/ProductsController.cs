
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Bulky.Models.ViewModels;
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
        return View(products);
    }

    public IActionResult Upsert(int? id)
    {
        ProductVM productVm = new()
        {
            CategoryList = _unit.Categories.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
            Product = new Product()
        };
        if (id is 0 or null)
        {
            //create
            return View(productVm);
        }
        else
        {
            //update
            productVm.Product = _unit.Products.Get(p => p.Id == id);
            return View(productVm);
        }
        
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVm, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            _unit.Products.Add(productVm.Product);
            _unit.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }
        else
        {
            productVm.CategoryList = _unit.Categories.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(productVm);
        }
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