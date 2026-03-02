using Microsoft.EntityFrameworkCore;

namespace UTB.Minute.Db
{
    public class MealsContext(DbContextOptions<MealsContext> options) : DbContext(options)
    {
        public DbSet<Meal> Meals { get; set; }
    }

}
