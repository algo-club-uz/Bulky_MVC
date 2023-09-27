
using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]

[Authorize(Roles = SD.Role_Admin)]
public class ProductsController : Controller
{
    private readonly IUnitOfWork _unit;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string includeName = "Category";
    public ProductsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unit = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        List<Product> products = _unit.Products.GetAll(includeName).ToList();
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
            string wwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwRootPath, @"images\product");

                if (!string.IsNullOrEmpty(productVm.Product.ImageUrl))
                {
                    //delete the old image
                    var oldImagePath = Path
                        .Combine(wwRootPath, productVm.Product.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(productPath,fileName),FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productVm.Product.ImageUrl = @"\images\product\" + fileName;
            }

            if (productVm.Product.Id is 0)
            {
                _unit.Products.Add(productVm.Product);
            }
            else
            {
                _unit.Products.Update(productVm.Product);
            }
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


    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Product> products = _unit.Products.GetAll(includeName).ToList();
        return Json(new {data = products });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var productToBeDeleted = _unit.Products.Get(u => u.Id == id);
        if (productToBeDeleted == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }

        if (!string.IsNullOrEmpty(productToBeDeleted.ImageUrl))
        {
            var oldImagePath = Path
                .Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
        }

        _unit.Products.Remove(productToBeDeleted);
        _unit.Save();

        return Json(new { success = true, message = "Delete Successful" });
    }

    #endregion

}