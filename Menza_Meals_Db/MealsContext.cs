using Microsoft.EntityFrameworkCore;

namespace Menza_Meals_Db
{
    public class MealsContext(DbContextOptions<MealsContext> options) : DbContext(options)
    {
        public DbSet<Meal> Meals { get; set; }
    }

}
