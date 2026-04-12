using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Contracts.Meals;
using UTB.Minute.Db.Entities;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Api collection")]
public class MealEndpointsTests : IClassFixture<TestFixture>
{
    private readonly TestFixture fixture;

    public MealEndpointsTests(TestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task CreateMeal_ShouldCreateMeal()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        var dto = new CreateMealDto("Smažený sýr", "S hranolkami");

        var response = await fixture.HttpClient.PostAsJsonAsync("/meals", dto, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdMeal = await response.Content.ReadFromJsonAsync<MealDto>(cancellationToken);
        Assert.NotNull(createdMeal);
        Assert.Equal("Smažený sýr", createdMeal!.Name);
        Assert.Equal("S hranolkami", createdMeal.Description);
        Assert.True(createdMeal.IsActive);

        await using var context = fixture.CreateContext();
        var mealInDb = await context.Meals.FirstOrDefaultAsync(m => m.Id == createdMeal.Id, cancellationToken);

        Assert.NotNull(mealInDb);
    }

    [Fact]
    public async Task GetMeal_ShouldReturnMeal()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int mealId;

        await using (var context = fixture.CreateContext())
        {
            var meal = new Meal
            {
                Name = "Kungpao",
                Description = "S nudlemi",
                IsActive = true
            };

            context.Meals.Add(meal);
            await context.SaveChangesAsync(cancellationToken);
            mealId = meal.Id;
        }

        var response = await fixture.HttpClient.GetAsync($"/meals/{mealId}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var mealDto = await response.Content.ReadFromJsonAsync<MealDto>(cancellationToken);

        Assert.NotNull(mealDto);
        Assert.Equal(mealId, mealDto!.Id);
        Assert.Equal("Kungpao", mealDto.Name);
        Assert.Equal("S nudlemi", mealDto.Description);
        Assert.True(mealDto.IsActive);
    }

    [Fact]
    public async Task UpdateMeal_ShouldUpdateMeal()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int mealId;

        await using (var context = fixture.CreateContext())
        {
            var meal = new Meal
            {
                Name = "Kungpao",
                Description = "S nudlemi",
                IsActive = true
            };

            context.Meals.Add(meal);
            await context.SaveChangesAsync(cancellationToken);
            mealId = meal.Id;
        }

        var dto = new UpdateMealDto("Gyros", "S rýží", true);

        var response = await fixture.HttpClient.PutAsJsonAsync($"/meals/{mealId}", dto, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedMeal = await response.Content.ReadFromJsonAsync<MealDto>(cancellationToken);

        Assert.NotNull(updatedMeal);
        Assert.Equal(mealId, updatedMeal!.Id);
        Assert.Equal("Gyros", updatedMeal.Name);
        Assert.Equal("S rýží", updatedMeal.Description);
    }

    [Fact]
    public async Task DeactivateMeal_ShouldDeactivateMeal()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var cancellationToken = cts.Token;

        await fixture.ResetDatabaseAsync(cancellationToken);

        int mealId;

        await using (var context = fixture.CreateContext())
        {
            var meal = new Meal
            {
                Name = "Gyros",
                Description = "S rýží",
                IsActive = true
            };

            context.Meals.Add(meal);
            await context.SaveChangesAsync(cancellationToken);
            mealId = meal.Id;
        }

        var response = await fixture.HttpClient.DeleteAsync($"/meals/{mealId}", cancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var verifyContext = fixture.CreateContext();
        var mealInDb = await verifyContext.Meals.FindAsync(new object[] { mealId }, cancellationToken);

        Assert.NotNull(mealInDb);
        Assert.False(mealInDb!.IsActive);
    }
}