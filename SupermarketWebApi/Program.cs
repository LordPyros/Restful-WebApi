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

        // Supermarket.NumberOfStaff and the actual number of staff associated with a supermarket are unrelated
        // (obviously this would not be the case in a real world application)

        // GetAllStock collection does not have the search query option (no point as stock contains no text to search for)

        // Upserting is only implemented in the PUT request on the supermarket controller as an example
        // (I would prefer that the user doesn't have to delete a new supermarket in situations where they were trying to
        // update an existing supermarket but accidently entered a non existent supermarket Id)

        
        // THINGS TO CORRECT
       
        // *** need an error message when supermarket doesn't exist during staff creation
        // *** too much stock triggers bad request instead of unprocessable during stock creation and update
        // *** staff member phone number is max length 20 instead of valid phone number attribute

        
        // *** REFACTORING TO DO

        // CreateLinksForProduct, CreateLinksForProduct, CreateProductResourceUri - Exist in both supermarket and product controllers
        // CreateLinksForStaffMember, CreateLinksForStaffMember, CreateStaffMemberResourceUri - Exist in both supermarket and staff controllers
        



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
