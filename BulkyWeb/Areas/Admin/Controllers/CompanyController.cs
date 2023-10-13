using Bulky.DataAccess.Repositories.IRepositories;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]

//[Authorize(Roles = SD.Role_Admin)]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unit;
    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unit = unitOfWork;
    }

    public IActionResult Index()
    {
        List<Company> companies = _unit.Companies.GetAll().ToList();
        return View(companies);
    }

    public IActionResult Upsert(int? id)
    {
        
        if (id is 0 or null)
        {
            //create
            return View(new Company());
        }
        else
        {
            //update
            Company company = _unit.Companies.Get(p => p.Id == id);
            return View(company);
        }
        
    }

    [HttpPost]
    public IActionResult Upsert(Company company)
    {
        if (ModelState.IsValid)
        {
            if (company.Id is 0)
            {
                _unit.Companies.Add(company);
            }
            else
            {
                _unit.Companies.Update(company);
            }
            _unit.Save();
            TempData["success"] = "Company created successfully";
            return RedirectToAction("Index");
        }
        else
        {
            return View(company);
        }
    }


    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Company> companies = _unit.Companies.GetAll().ToList();
        return Json(new {data = companies });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var companyToBeDeleted = _unit.Companies.Get(u => u.Id == id);
        if (companyToBeDeleted == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }


        _unit.Companies.Remove(companyToBeDeleted);
        _unit.Save();

        return Json(new { success = true, message = "Delete Successful" });
    }

    #endregion

}