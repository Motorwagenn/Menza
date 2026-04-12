using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Contracts.Enums;
using UTB.Minute.Contracts.Meals;
using UTB.Minute.Contracts.Menu;
using UTB.Minute.Contracts.Orders;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<MinuteDbContext>("database");

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

//endpoints meal managment
app.MapPost("/meals", MealEndpoints.CreateMeal);
app.MapGet("/meals", MealEndpoints.GetMeals);
app.MapGet("/meals/{id}", MealEndpoints.GetMeal);
app.MapGet("/meals/active", MealEndpoints.GetActiveMeals);
app.MapPut("/meals/{id}", MealEndpoints.UpdateMeal);
app.MapDelete("/meals/{id}", MealEndpoints.DeactivateMeal);
//endopints orders 
app.MapPost("/orders", OrderEndpoints.CreateOrder);
app.MapGet("/orders", OrderEndpoints.GetOrders);
app.MapPut("/orders/{id}/status", OrderEndpoints.UpdateOrderStatus);
app.MapGet("/orders/{id}", OrderEndpoints.GetOrder);
//endpoints menu
app.MapGet("/menu", MenuEndpoints.GetMenuItems);
app.MapPost("/menu", MenuEndpoints.CreateMenuItem);
app.MapGet("/menu/{id}", MenuEndpoints.GetMenuItem);
app.MapPut("/menu/{id}", MenuEndpoints.UpdateMenuItem);
app.MapDelete("/menu/{id}", MenuEndpoints.DeleteMenuItem);


app.Run();

public static class MealEndpoints
{
    //CRUD methods

    public static async Task<Created<MealDto>> CreateMeal(CreateMealDto dto, MinuteDbContext context)
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
    public static async Task<Results<Ok<MealDto>, NotFound>> UpdateMeal(int id, UpdateMealDto dto, MinuteDbContext context)
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

public static class OrderEndpoints
{
    public static async Task<Results<Created<OrderDto>, NotFound, BadRequest>> CreateOrder(
    CreateOrderDto dto,
    MinuteDbContext context)
    {
        var menuItem = await context.MenuItems
            .Include(m => m.Meal)
            .FirstOrDefaultAsync(m => m.Id == dto.MenuItemId);

        if (menuItem == null)
            return TypedResults.NotFound();

        if (menuItem.PortionsAvailable <= 0)
            return TypedResults.BadRequest();

        // lowering portions
        menuItem.PortionsAvailable--;

        var order = new Order
        {
            MenuItemId = menuItem.Id,
            Status = OrderStatus.Preparing,
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var result = new OrderDto(
            order.Id,
            menuItem.Id,
            menuItem.Meal.Name,
            order.Status,
            order.CreatedAt
        );

        return TypedResults.Created($"/orders/{order.Id}", result);
    }

    public static async Task<Ok<List<OrderDto>>> GetOrders(MinuteDbContext context)
    {
        var orders = await context.Orders
            .Include(o => o.MenuItem)
            .ThenInclude(m => m.Meal)
            .Where(o => o.Status != OrderStatus.Completed)
            .Select(o => new OrderDto(
                o.Id,
                o.MenuItemId,
                o.MenuItem.Meal.Name,
                o.Status,
                o.CreatedAt
            ))
            .ToListAsync();

        return TypedResults.Ok(orders);
    }

    public static async Task<Results<Ok<OrderDto>, NotFound>> UpdateOrderStatus(
    int id,
    UpdateOrderStatusDto dto,
    MinuteDbContext context)
    {
        var order = await context.Orders
            .Include(o => o.MenuItem)
            .ThenInclude(m => m.Meal)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return TypedResults.NotFound();

        order.Status = dto.Status;

        await context.SaveChangesAsync();

        var result = new OrderDto(
            order.Id,
            order.MenuItemId,
            order.MenuItem.Meal.Name,
            order.Status,
            order.CreatedAt
        );

        return TypedResults.Ok(result);
    }
    public static async Task<Results<Ok<OrderDto>, NotFound>> GetOrder(int id, MinuteDbContext context)
    {
        var order = await context.Orders
            .Include(o => o.MenuItem)
            .ThenInclude(m => m.Meal)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return TypedResults.NotFound();

        var result = new OrderDto(
            order.Id,
            order.MenuItemId,
            order.MenuItem.Meal.Name,
            order.Status,
            order.CreatedAt
        );

        return TypedResults.Ok(result);
    }
}
public static class MenuEndpoints
{
    // GET /menu
    public static async Task<Ok<List<MenuItemDto>>> GetMenuItems(MinuteDbContext context)
    {
        var menu = await context.MenuItems
            .Include(m => m.Meal)
            .Select(m => new MenuItemDto(
                m.Id,
                m.Date,
                m.MealId,
                m.Meal.Name,
                m.PortionsAvailable
            ))
            .ToListAsync();

        return TypedResults.Ok(menu);
    }
    public static async Task<Results<Created<MenuItemDto>, NotFound>> CreateMenuItem(CreateMenuItemDto dto, MinuteDbContext context)
    {
        var meal = await context.Meals.FindAsync(dto.MealId);
        if (meal == null)
            return TypedResults.NotFound();


        var menuItem = new MenuItem
        {
            MealId = dto.MealId,
            Date = dto.Date,
            PortionsAvailable = dto.AvailablePortions
        };

        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();

        var result = new MenuItemDto(
            menuItem.Id,
            menuItem.Date,
            menuItem.MealId,
            meal.Name,
            menuItem.PortionsAvailable
        );

        return TypedResults.Created($"/menu/{menuItem.Id}", result);
    }
    public static async Task<Results<Ok<MenuItemDto>, NotFound>> GetMenuItem(int id, MinuteDbContext context)
    {
        var menuItem = await context.MenuItems
            .Include(m => m.Meal)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menuItem == null)
            return TypedResults.NotFound();

        var result = new MenuItemDto(
            menuItem.Id,
            menuItem.Date,
            menuItem.MealId,
            menuItem.Meal.Name,
            menuItem.PortionsAvailable
        );

        return TypedResults.Ok(result);
    }
    public static async Task<Results<Ok<MenuItemDto>, NotFound>> UpdateMenuItem(
    int id,
    UpdateMenuItemDto dto,
    MinuteDbContext context)
    {
        var menuItem = await context.MenuItems.FindAsync(id);
        if (menuItem == null)
            return TypedResults.NotFound();

        menuItem.Date = dto.Date;
        menuItem.MealId = dto.MealId;
        menuItem.PortionsAvailable = dto.AvailablePortions;

        await context.SaveChangesAsync();


        var meal = await context.Meals.FindAsync(menuItem.MealId);

        var result = new MenuItemDto(
            menuItem.Id,
            menuItem.Date,
            menuItem.MealId,
            meal?.Name ?? string.Empty,
            menuItem.PortionsAvailable
        );

        return TypedResults.Ok(result);
    }
    public static async Task<Results<NoContent, NotFound>> DeleteMenuItem(int id, MinuteDbContext context)
    {
        var menuItem = await context.MenuItems.FindAsync(id);
        if (menuItem == null)
            return TypedResults.NotFound();

        context.MenuItems.Remove(menuItem);
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
