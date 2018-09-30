using System.Linq;
using TelephoneCallsWebApplication.Models.DataGeneration;

namespace TelephoneCallsWebApplication.Data
{
    public static class DbInitializer
    {
        public static void Initialize(TelephoneCallsContext context)
        {
            //To drop the database
            //context.Database.EnsureDeleted();

            context.Database.EnsureCreated();

            //Look for any calls
            if (context.Calls.Any())
            {
                return;   // DB has been seeded
            }

            var calls = CallGeneration.GenerateCalls();
            context.Calls.AddRange(calls);
            context.SaveChanges();
        }
    }
}
