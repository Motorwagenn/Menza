using UTB.Minute.Contracts.Enums;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<MinuteDbContext>("database");

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.MapPost("/reset-db", async (MinuteDbContext context) =>
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    Meal m1 = new() { Name = "Rezen", Description = "description pro rizky",IsActive=true};
    Meal m2 = new() { Name = "Palacinky", Description="description pro palacinky",IsActive=true};
    Meal m3 = new() { Name = "Salát", Description = "Zeleninový salát", IsActive = false };

    context.Meals.AddRange(m1, m2,m3);
    await context.SaveChangesAsync();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    MenuItem menu1 = new() { MealId = m1.Id, Date = today, PortionsAvailable = 5 };
    MenuItem menu2 = new() { MealId = m2.Id, Date = today, PortionsAvailable = 0 };
    MenuItem menu3 = new() { MealId = m3.Id, Date = today, PortionsAvailable = 10 };

    context.MenuItems.AddRange(menu1, menu2,menu3);
    await context.SaveChangesAsync();

    var order1 = new Order { MenuItemId = menu1.Id, Status = OrderStatus.Preparing, CreatedAt = DateTime.UtcNow };
    var order2 = new Order { MenuItemId = menu1.Id, Status = OrderStatus.Ready, CreatedAt = DateTime.UtcNow };
    var order3 = new Order { MenuItemId = menu3.Id, Status = OrderStatus.Completed, CreatedAt = DateTime.UtcNow };

    context.Orders.AddRange(order1, order2, order3);
    await context.SaveChangesAsync();
});

app.Run();

