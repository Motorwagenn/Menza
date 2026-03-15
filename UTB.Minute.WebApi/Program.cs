using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Contracts;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<MinuteDbContext>("database");

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

//endpointy pro meal managment
app.MapPost("/meals", MealEndpoints.CreateMeal);
app.MapGet("/meals", MealEndpoints.GetMeals);
app.MapGet("/meals/{id}", MealEndpoints.GetMeal);
app.MapGet("/meals/active", MealEndpoints.GetActiveMeals);
app.MapPut("/meals/{id}", MealEndpoints.UpdateMeal);
app.MapDelete("/meals/{id}", MealEndpoints.DeactivateMeal);
//endopinty pro orders 

app.Run();

public static class MealEndpoints
{
    //CRUD metody

    public static async Task<Created<MealDto>> CreateMeal(CreateMealDto dto,MinuteDbContext context)
    {
        Meal meal = new()
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true
        };

        context.Meals.Add(meal);
        await context.SaveChangesAsync();

        MealDto result = new(meal.Id, meal.Name, meal.Description, meal.IsActive);

        return TypedResults.Created($"/meals/{meal.Id}", result);
    }
    public static async Task<Ok<List<MealDto>>> GetMeals(MinuteDbContext context)
    {
        var meals = await context.Meals
            .Select(m => new MealDto(m.Id, m.Name, m.Description, m.IsActive))
            .ToListAsync();

        return TypedResults.Ok(meals);
    }
    public static async Task<Results<Ok<MealDto>, NotFound>> UpdateMeal(int id,UpdateMealDto dto,MinuteDbContext context)
    {
        var meal = await context.Meals.FindAsync(id);
        if (meal == null) return TypedResults.NotFound();

        meal.Name = dto.Name;
        meal.Description = dto.Description;
        meal.IsActive = dto.IsActive;

        await context.SaveChangesAsync();

        MealDto result = new(meal.Id, meal.Name, meal.Description, meal.IsActive);
        return TypedResults.Ok(result);
    }
    public static async Task<Results<NoContent, NotFound>> DeactivateMeal(int id, MinuteDbContext context)
    {
        var meal = await context.Meals.FindAsync(id);
        if (meal == null) return TypedResults.NotFound();

        meal.IsActive = false;
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    }
    public static async Task<Results<Ok<MealDto>, NotFound>> GetMeal(int id, MinuteDbContext context)
    {
        var meal = await context.Meals.FindAsync(id);

        if (meal == null)
            return TypedResults.NotFound();

        var result = new MealDto(meal.Id, meal.Name, meal.Description, meal.IsActive);

        return TypedResults.Ok(result);
    }
    public static async Task<Ok<List<MealDto>>> GetActiveMeals(MinuteDbContext context)
    {
        var meals = await context.Meals
            .Where(m => m.IsActive)
            .Select(m => new MealDto(m.Id, m.Name, m.Description, m.IsActive))
            .ToListAsync();

        return TypedResults.Ok(meals);
    }
}

