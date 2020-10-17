using DAL.DAL;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Services
{
    public static class DalService
    {
        public static async Task InsertResult(WatermarkingResults result)
        {
            await using var context= new WatermarkingContext();
            await context.WatermarkingResults.AddAsync(result);
            await context.SaveChangesAsync();
        }

        public static async Task InsertResults(IList<WatermarkingResults> results)
        {
            await using var context = new WatermarkingContext();
            await context.WatermarkingResults.AddRangeAsync(results);
            await context.SaveChangesAsync();
        }

        public static async Task<List<WatermarkingResults>> GetAllResults()
        {
            await using var context = new WatermarkingContext();
            return await context.WatermarkingResults.AsNoTracking().ToListAsync();
        }
    }
}
