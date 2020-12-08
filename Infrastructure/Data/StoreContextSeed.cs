using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data
{
    public class StoreContextSeed
    {
        public static async Task SeedAsync(StoreContext context, ILoggerFactory loggerFactory)
        {
            try
            {   
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (!context.Appointments.Any())
                {
                    var appointmentsData = File.ReadAllText(path + @"/Data/SeedData/Appointments.json");

                    var appointments = JsonSerializer.Deserialize<List<Appointment>>(appointmentsData);

                    foreach (var item in appointments)
                    {
                        context.Appointments.Add(item);
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<StoreContextSeed>();
                logger.LogError(ex, "An error occurred during StoreContext seed");
            }
        }
    }
}