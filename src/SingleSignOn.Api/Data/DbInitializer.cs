using SingleSignOn.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SingleSignOn.Api.Data
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string AdminRoleName = "Admin";
        private readonly string UserRoleName = "Member";

        public DbInitializer(ApplicationDbContext context,
          UserManager<User> userManager,
          RoleManager<IdentityRole> roleManager
          )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

        }

        public RoleManager<IdentityRole> RoleManager => _roleManager;

        //Seed data if data on database is null
        public async Task Seed()
        {
            #region Role

            if (!RoleManager.Roles.Any())
            {
                await RoleManager.CreateAsync(new IdentityRole
                {
                    Id = AdminRoleName,
                    Name = AdminRoleName,
                    NormalizedName = AdminRoleName.ToUpper(),
                });
                await RoleManager.CreateAsync(new IdentityRole
                {
                    Id = UserRoleName,
                    Name = UserRoleName,
                    NormalizedName = UserRoleName.ToUpper(),
                });
            }

            #endregion

            #region User

            if (!_userManager.Users.Any())
            {
                var result1 = await _userManager.CreateAsync(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "admin",
                    FirstName = "Admin",
                    LastName = "",
                    Email = "admin@admin.com",
                    LockoutEnabled = false,
                    Dob = DateTime.Parse("1998/04/11"),
                    CreateDate = DateTime.UtcNow,
                    AvatarUri = "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png",
                }, "Admin@123");
                if (result1.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync("admin");
                    await _userManager.AddToRoleAsync(user, AdminRoleName);
                    var role = await _roleManager.FindByIdAsync(AdminRoleName);
                    var claims = new List<Claim>();
                    claims.Add(new Claim("SSO_SERVER", "VIEW"));
                    claims.Add(new Claim("SSO_SERVER", "CREATE"));
                    claims.Add(new Claim("SSO_SERVER", "UPDATE"));
                    claims.Add(new Claim("SSO_SERVER", "DELETE"));
                    claims.Add(new Claim("Swagger SSO Server", "true"));
                    claims.Add(new Claim("Angular Admin Dashboard", "true"));
                    claims.Add(new Claim("Angular User Profile", "true"));
                    foreach (var claim in claims)
                    {
                        await _roleManager.AddClaimAsync(role, claim);
                    }

                }

                var result2 = await _userManager.CreateAsync(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "xdxg",
                    FirstName = "Tịnh",
                    LastName = "Phạm Văn",
                    Email = "tinh_pham@outlook.com",
                    LockoutEnabled = false,
                    Dob = DateTime.Parse("1998/04/11"),
                    CreateDate = DateTime.UtcNow,
                    AvatarUri = "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png",
                }, "!kAa36qDc");
                if (result2.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync("xdxg");
                    await _userManager.AddToRoleAsync(user, UserRoleName);
                    var role = await _roleManager.FindByIdAsync(UserRoleName);
                    var claims = new List<Claim>();
                    claims.Add(new Claim("SSO_SERVER", "VIEW"));
                    claims.Add(new Claim("Swagger SSO Server", "true"));
                    claims.Add(new Claim("Angular Admin Dashboard", "true"));
                    claims.Add(new Claim("Angular User Profile", "true"));
                    foreach (var claim in claims)
                    {
                        await _roleManager.AddClaimAsync(role, claim);
                    }

                }
            }

            #endregion
            await _context.SaveChangesAsync();
        }
    }
}