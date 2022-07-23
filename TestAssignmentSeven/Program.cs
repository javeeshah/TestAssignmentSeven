using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using TestAssignmentSeven.HttpServices;

namespace TestAssignmentSeven
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var host = CreateDefaultBuilder(args).Build();

                using IServiceScope serviceScope = host.Services.CreateScope();
                IServiceProvider serviceProvider = serviceScope.ServiceProvider;
                var personInstance = serviceProvider.GetRequiredService<IPersonService>();
                await personInstance.GetPersons();

                host.Run();               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        }

        static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)             
                .ConfigureAppConfiguration(app =>
                {
                    app.AddJsonFile("appsettings.json");
                    app.AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IPersonService, PersonService>();
                    services.AddHttpClient();                                     
                });               
        }        
    }
}
