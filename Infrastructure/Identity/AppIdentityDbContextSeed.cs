using System.Linq;
using System.Threading.Tasks;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity
{
    public class AppIdentityDbContextSeed
    {
        public static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var user1 = new AppUser
                {
                    UserName = "Admin_1",
                    Email = "admin1@gmail.com",
                    IsAdmin = true,
                    IsEmployee = false
                };

                await userManager.CreateAsync(user1, "Password");

                var user2 = new AppUser
                {
                    UserName = "Funcionario_1",
                    Email = "funcionario1@gmail.com",
                    IsAdmin = false,
                    IsEmployee = true
                };

                await userManager.CreateAsync(user2, "Password");

                var user3 = new AppUser
                {
                    UserName = "Cliente_1",
                    Email = "cliente1@gmail.com",
                    IsAdmin = false,
                    IsEmployee = false
                };

                await userManager.CreateAsync(user3, "Password");

                var user4 = new AppUser
                {
                    UserName = "Cliente_2",
                    Email = "cliente2@gmail.com",
                    IsAdmin = false,
                    IsEmployee = false
                };

                await userManager.CreateAsync(user4, "Password");

                var user5 = new AppUser
                {
                    UserName = "Cliente_3",
                    Email = "cliente3@gmail.com",
                    IsAdmin = false,
                    IsEmployee = false
                };

                await userManager.CreateAsync(user5, "Password");
            }
        }
    }
}