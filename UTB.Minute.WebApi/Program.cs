using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Contracts;
using UTB.Minute.Db;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<MealsContext>("database");

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


//app.MapPost();
//app.MapGet();
//app.MapPut();
//app.MapDelete();

app.Run();

public static class WebApiVersion1
{
    //CRUD metody

    public static async Task<Created<MealDto>> CreateMeal(MealDto mealDto, MealsContext context)
    {
        Meal meal = new() { Name = mealDto.Name };

        context.Meals.Add(meal);

        await context.SaveChangesAsync();

        MealDto resultDto = new(meal.Id, meal.Name);

        return TypedResults.Created($"/meals/{resultDto.Id}", resultDto);
    }


}

