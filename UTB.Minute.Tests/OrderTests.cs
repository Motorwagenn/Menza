using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Contracts.Enums;
using UTB.Minute.Contracts.Orders;
using UTB.Minute.Db.Entities;
using Xunit;

namespace UTB.Minute.Tests;

[Collection("Api collection")]
public class OrderEndpointsTests : IClassFixture<TestFixture>
{
    private readonly TestFixture fixture;

    public OrderEndpointsTests(TestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task CreateOrder_ShouldCreateOrder()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int menuItemId;

        await using (var context = fixture.CreateContext())
        {
            var meal = new Meal
            {
                Name = "Kuřecí steak",
                Description = "S rýží",
                IsActive = true
            };

            context.Meals.Add(meal);
            await context.SaveChangesAsync(cancellationToken);

            var menuItem = new MenuItem
            {
                MealId = meal.Id,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                PortionsAvailable = 3
            };

            context.MenuItems.Add(menuItem);
            await context.SaveChangesAsync(cancellationToken);
            menuItemId = menuItem.Id;
        }

        var dto = new CreateOrderDto(menuItemId);

        var response = await fixture.HttpClient.PostAsJsonAsync("/orders", dto, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdOrder = await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken);

        Assert.NotNull(createdOrder);
        Assert.Equal(menuItemId, createdOrder!.MenuItemId);
        Assert.Equal(OrderStatus.Preparing, createdOrder.Status);
        Assert.Equal("Kuřecí steak", createdOrder.MealName);
    }

    [Fact]
    public async Task GetOrder_ShouldReturnOrder()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int orderId;

        await using (var context = fixture.CreateContext())
        {
            var meal = new Meal
            {
                Name = "Guláš",
                Description = "S knedlíkem",
                IsActive = true
            };

            context.Meals.Add(meal);
            await context.SaveChangesAsync(cancellationToken);

            var menuItem = new MenuItem
            {
                MealId = meal.Id,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                PortionsAvailable = 5
            };

            context.MenuItems.Add(menuItem);
            await context.SaveChangesAsync(cancellationToken);

            var order = new Order
            {
                MenuItemId = menuItem.Id,
                Status = OrderStatus.Preparing,
                CreatedAt = DateTime.UtcNow
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync(cancellationToken);
            orderId = order.Id;
        }

        var response = await fixture.HttpClient.GetAsync($"/orders/{orderId}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orderDto = await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken);

        Assert.NotNull(orderDto);
        Assert.Equal(orderId, orderDto!.Id);
        Assert.Equal(OrderStatus.Preparing, orderDto.Status);
        Assert.Equal("Guláš", orderDto.MealName);
    }

    [Fact]
    public async Task GetOrders_ShouldReturnOnlyNonCompletedOrders()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        await using (var context = fixture.CreateContext())
        {
            var meal = new Meal
            {
                Name = "Guláš",
                Description = "S knedlíkem",
                IsActive = true
            };

            context.Meals.Add(meal);
            await context.SaveChangesAsync(cancellationToken);

            var menuItem = new MenuItem
            {
                MealId = meal.Id,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                PortionsAvailable = 10
            };

            context.MenuItems.Add(menuItem);
            await context.SaveChangesAsync(cancellationToken);

            context.Orders.AddRange(
                new Order
                {
                    MenuItemId = menuItem.Id,
                    Status = OrderStatus.Preparing,
                    CreatedAt = DateTime.UtcNow
                },
                new Order
                {
                    MenuItemId = menuItem.Id,
                    Status = OrderStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync(cancellationToken);
        }

        var response = await fixture.HttpClient.GetAsync("/orders", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(cancellationToken);

        Assert.NotNull(orders);
        Assert.Single(orders!);
        Assert.Equal(OrderStatus.Preparing, orders[0].Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_ShouldUpdateOrderStatus()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int orderId;

        await using (var context = fixture.CreateContext())
        {
            var meal = new Meal
            {
                Name = "Svíčková",
                Description = "S knedlíkem",
                IsActive = true
            };

            context.Meals.Add(meal);
            await context.SaveChangesAsync(cancellationToken);

            var menuItem = new MenuItem
            {
                MealId = meal.Id,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                PortionsAvailable = 6
            };

            context.MenuItems.Add(menuItem);
            await context.SaveChangesAsync(cancellationToken);

            var order = new Order
            {
                MenuItemId = menuItem.Id,
                Status = OrderStatus.Preparing,
                CreatedAt = DateTime.UtcNow
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync(cancellationToken);
            orderId = order.Id;
        }

        var dto = new UpdateOrderStatusDto(OrderStatus.Completed);

        var response = await fixture.HttpClient.PutAsJsonAsync($"/orders/{orderId}/status", dto, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedOrder = await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken);

        Assert.NotNull(updatedOrder);
        Assert.Equal(orderId, updatedOrder!.Id);
        Assert.Equal(OrderStatus.Completed, updatedOrder.Status);

        await using var verifyContext = fixture.CreateContext();
        var orderInDb = await verifyContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        Assert.NotNull(orderInDb);
        Assert.Equal(OrderStatus.Completed, orderInDb!.Status);
    }
}