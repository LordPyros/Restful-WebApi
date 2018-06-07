using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;

namespace SupermarketWebApi
{
    public class Program
    {
        // Data restrictions (restricted in models only, database can handle larger values)
        // A price cannot be more than $10,000 
        // A Supermarket cannot have more than 10,000 staff
        // A Supermarket cannot have more than 100,000,000 of a product

        // GetAllStock collection does not have the search query option (no point as stock contains no text to search for)

        // * may have to update logging setting in appsettings.json 

        // * skipped upserting, patch, filtering

        // *** product price can be minus up to -1

        // *** need an error message when supermarket doesn't exist during staff creation

        // *** too much stock triggers bad request instead of unprocessable during stock creation and update

        // *** staff member phone number is max length 20 instead of valid phone number attribute

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseNLog()
                .Build();
    }
}
