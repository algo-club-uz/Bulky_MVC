using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.DbInitializer;

public class DbInitializer: IDbInitializer
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _appDbContext;

    public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext appDbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _appDbContext = appDbContext;
    }

    public void Initialize()
    {
        try
        {
            if (_appDbContext.Database.GetPendingMigrations().Count() > 0)
            {
                _appDbContext.Database.Migrate();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
        {
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "fuga_02",
                Email = "fuga02@algoclub.com",
                Name = "Maruf Berdiev",
                PhoneNumber = "13708353402",
                StreetAddress = "CQPT",
                State = "Na'nan",
                PostalCode = "1245",
                City = "Chongqing"
            }, "coca2002").GetAwaiter().GetResult();
        }

        return;

    }
}