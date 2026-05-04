using System.Net.Http.Json;
using UTB.Minute.Contracts.Meals;
using UTB.Minute.Contracts.Menu;

namespace UTB.Minute.AdminClient
{
    public class AdminService(HttpClient httpClient)
    {
        // Meals
        public async Task<MealDto[]?> GetMealsAsync()
        {
            MealDto[]? meals = await httpClient.GetFromJsonAsync<MealDto[]>("/meals");
            return meals;
        }

        public async Task CreateMealAsync(CreateMealDto dto)
        {
            await httpClient.PostAsJsonAsync("/meals", dto);
        }

        public async Task UpdateMealAsync(int id, UpdateMealDto dto)
        {
            await httpClient.PutAsJsonAsync($"/meals/{id}", dto);
        }

        public async Task DeactivateMealAsync(int id)
        {
            await httpClient.DeleteAsync($"/meals/{id}");
        }

        // Menu
        public async Task<MenuItemDto[]?> GetMenuItemsAsync()
        {
            MenuItemDto[]? menuItems = await httpClient.GetFromJsonAsync<MenuItemDto[]>("/menu");
            return menuItems;
        }

        public async Task CreateMenuItemAsync(CreateMenuItemDto dto)
        {
            await httpClient.PostAsJsonAsync("/menu", dto);
        }

        public async Task UpdateMenuItemAsync(int id, UpdateMenuItemDto dto)
        {
            await httpClient.PutAsJsonAsync($"/menu/{id}", dto);
        }

        public async Task DeleteMenuItemAsync(int id)
        {
            await httpClient.DeleteAsync($"/menu/{id}");
        }
    }
}