using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories;

[BindProperties]
public class EditModel : PageModel
{
    private readonly AppDbContext _appDbContext;

    public Category Category { get; set; }
    public EditModel(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void OnGet(int? id)
    {
        if (id is not null and not 0)
        {
            Category = _appDbContext.Categories.FirstOrDefault(c => c.Id == id);
        }
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            _appDbContext.Categories.Update(Category!);
            _appDbContext.SaveChanges();
            TempData["success"] = "Category updated      successfully";
            return RedirectToPage("index");
        }

        return Page();
    }
}