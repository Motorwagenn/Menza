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
    Meal m2 = new() { Name = "Palacinky", Description="description pro palacinky",IsActive=false};

    context.Meals.AddRange(m1, m2);

    await context.SaveChangesAsync();
});

app.Run();

