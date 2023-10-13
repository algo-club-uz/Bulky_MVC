using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
//[Authorize(Roles = SD.Role_Admin)]
public class CategoryController : Controller
{
    private readonly IUnitOfWork _unit;

    public CategoryController(IUnitOfWork unit)
    {
        _unit = unit;
    }

    public IActionResult Index()
    {
        List<Category> categories = _unit.Categories.GetAll().ToList();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Category category)
    {
        if (category.Name == category.DisplayOrder.ToString())
            ModelState.AddModelError("name", "Name can't be equal with order's name");
        if (ModelState.IsValid)
        {
            _unit.Categories.Add(category);
            _unit.Save();
            TempData["success"] = "Category created successfully";
            return RedirectToAction("Index", "Category");
        }
        return View();
    }

    public IActionResult Edit(int? id)
    {
        if (id == null && id == 0)
        {
            return NotFound();
        }

        var category = _unit.Categories.Get(u => u.Id == id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost]
    public IActionResult Edit(Category category)
    {
        if (ModelState.IsValid)
        {
            _unit.Categories.Update(category);
            _unit.Save();
            TempData["success"] = "Category updated successfully";
            return RedirectToAction("Index", "Category");
        }
        return View();
    }
    public IActionResult Delete(int? id)
    {
        if (id == null && id == 0)
        {
            return NotFound();
        }

        var category = _unit.Categories.Get(u => u.Id == id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeletePost(int? id)
    {
        var category = _unit.Categories.Get(u => u.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        _unit.Categories.Remove(category);
        _unit.Save();
        TempData["success"] = "Category deleted successfully";
        return RedirectToAction("Index", "Category");
    }

}