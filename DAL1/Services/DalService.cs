
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
namespace DAL.Services
{
    public static class DalService
    {
        public static async Task InsertResult(WatermarkingResults result)
        {         
            using (WatermarkingEntities context = new WatermarkingEntities())
            {
                context.WatermarkingResults.Add(result);
                await context.SaveChangesAsync();
            }
        }

        public static async Task InsertResults(IList<WatermarkingResults> results)
        {
            using (WatermarkingEntities context = new WatermarkingEntities())
            {
                context.WatermarkingResults.AddRange(results);
                await context.SaveChangesAsync();
            }

        }

        public static async Task<List<WatermarkingResults>> GetAllResults()
        {
            using (WatermarkingEntities context = new WatermarkingEntities())
            {
                return await context.WatermarkingResults.AsNoTracking().ToListAsync();
            }            
        }
    }
}
