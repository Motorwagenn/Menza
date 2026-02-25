using Menza_Meals_Db;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<MealsContext>("database");

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.MapPost("/reset-db", async (MealsContext context) =>
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    Meal m1 = new() { Name = "Rezen", Id = 1 };
    Meal m2 = new() { Name = "Palacinky", Id = 2 };

    context.Meals.AddRange(m1, m2);

    int changed = await context.SaveChangesAsync();
});

app.Run();

