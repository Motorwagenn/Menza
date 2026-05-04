using System.Net.Http.Json;
using UTB.Minute.Contracts.Menu;
using UTB.Minute.Contracts.Orders;

namespace UTB.Minute.CanteenClient
{
    public class CanteenService(HttpClient httpClient)
    {
        public async Task<MenuItemDto[]?> GetMenuItemsAsync()
        {
            MenuItemDto[]? items = await httpClient.GetFromJsonAsync<MenuItemDto[]>("/menu");
            return items;
        }

        public async Task CreateOrderAsync(CreateOrderDto dto)
        {
            await httpClient.PostAsJsonAsync("/orders", dto);
        }

        public async Task<OrderDto[]?> GetOrdersAsync()
        {
            OrderDto[]? orders = await httpClient.GetFromJsonAsync<OrderDto[]>("/orders");
            return orders;
        }

        public async Task UpdateOrderStatusAsync(int id, UpdateOrderStatusDto dto)
        {
            await httpClient.PutAsJsonAsync($"/orders/{id}/status", dto);
        }
    }
}