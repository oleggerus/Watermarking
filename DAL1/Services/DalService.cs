
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        public static async Task<List<WatermarkingResults>> GetAllResultByMode(int mode)
        {
            using (WatermarkingEntities context = new WatermarkingEntities())
            {
                return await context.WatermarkingResults.AsNoTracking().Where(x => x.Mode == mode).ToListAsync();
            }
        }

        public static async Task<List<WatermarkingResults>> GetAllResultByMode(List<int> modes)
        {
            using (WatermarkingEntities context = new WatermarkingEntities())
            {
                return await context.WatermarkingResults.AsNoTracking().Where(x => modes.Contains((int)x.Mode)).ToListAsync();
            }
        }
    }
}
