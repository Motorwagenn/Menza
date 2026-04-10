using System.Net;
using System.Net.Http.Json;
using UTB.Minute.Contracts.Menu;
using UTB.Minute.Db.Entities;
using Xunit;

namespace UTB.Minute.Tests;

[Collection("Api collection")]
public class MenuEndpointsTests : IClassFixture<TestFixture>
{
    private readonly TestFixture fixture;

    public MenuEndpointsTests(TestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task CreateMenuItem_ShouldCreateMenuItem()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int mealId;

        await using (var context = fixture.CreateContext())
        {
            var meal = new Meal
            {
                Name = "Řízek",
                Description = "S bramborem",
                IsActive = true
            };

            context.Meals.Add(meal);
            await context.SaveChangesAsync(cancellationToken);
            mealId = meal.Id;
        }

        var dto = new CreateMenuItemDto(
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            mealId,
            10);

        var response = await fixture.HttpClient.PostAsJsonAsync("/menu", dto, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdMenuItem = await response.Content.ReadFromJsonAsync<MenuItemDto>(cancellationToken);

        Assert.NotNull(createdMenuItem);
        Assert.Equal(mealId, createdMenuItem!.MealId);
        Assert.Equal(10, createdMenuItem.AvailablePortions);
    }

    [Fact]
    public async Task GetMenuItem_ShouldReturnMenuItem()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int menuItemId;

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
                PortionsAvailable = 7
            };

            context.MenuItems.Add(menuItem);
            await context.SaveChangesAsync(cancellationToken);
            menuItemId = menuItem.Id;
        }

        var response = await fixture.HttpClient.GetAsync($"/menu/{menuItemId}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var menuItemDto = await response.Content.ReadFromJsonAsync<MenuItemDto>(cancellationToken);

        Assert.NotNull(menuItemDto);
        Assert.Equal(menuItemId, menuItemDto!.Id);
        Assert.Equal(7, menuItemDto.AvailablePortions);
        Assert.Equal("Guláš", menuItemDto.MealName);
    }

    [Fact]
    public async Task UpdateMenuItem_ShouldUpdateMenuItem()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int meal1Id;
        int meal2Id;
        int menuItemId;

        await using (var context = fixture.CreateContext())
        {
            var meal1 = new Meal
            {
                Name = "Svíčková",
                Description = "Na smetaně",
                IsActive = true
            };

            var meal2 = new Meal
            {
                Name = "Rajská omáčka",
                Description = "S masovými kuličkami",
                IsActive = true
            };

            context.Meals.AddRange(meal1, meal2);
            await context.SaveChangesAsync(cancellationToken);

            meal1Id = meal1.Id;
            meal2Id = meal2.Id;

            var menuItem = new MenuItem
            {
                MealId = meal1Id,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                PortionsAvailable = 5
            };

            context.MenuItems.Add(menuItem);
            await context.SaveChangesAsync(cancellationToken);
            menuItemId = menuItem.Id;
        }

        var dto = new UpdateMenuItemDto(
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            meal2Id,
            15);

        var response = await fixture.HttpClient.PutAsJsonAsync($"/menu/{menuItemId}", dto, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedMenuItem = await response.Content.ReadFromJsonAsync<MenuItemDto>(cancellationToken);

        Assert.NotNull(updatedMenuItem);
        Assert.Equal(menuItemId, updatedMenuItem!.Id);
        Assert.Equal(meal2Id, updatedMenuItem.MealId);
        Assert.Equal(15, updatedMenuItem.AvailablePortions);
    }

    [Fact]
    public async Task DeleteMenuItem_ShouldDeleteMenuItem()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int menuItemId;

        await using (var context = fixture.CreateContext())
        {
            var meal = new Meal
            {
                Name = "Mazané jídlo",
                Description = "s pečivem",
                IsActive = true
            };

            context.Meals.Add(meal);
            await context.SaveChangesAsync(cancellationToken);

            var menuItem = new MenuItem
            {
                MealId = meal.Id,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                PortionsAvailable = 4
            };

            context.MenuItems.Add(menuItem);
            await context.SaveChangesAsync(cancellationToken);
            menuItemId = menuItem.Id;
        }

        var response = await fixture.HttpClient.DeleteAsync($"/menu/{menuItemId}", cancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var verifyContext = fixture.CreateContext();
        var menuItemInDb = await verifyContext.MenuItems.FindAsync(new object[] { menuItemId }, cancellationToken);

        Assert.Null(menuItemInDb);
    }
}