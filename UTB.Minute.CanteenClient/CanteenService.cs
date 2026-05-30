using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
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

        public async IAsyncEnumerable<OrderNotificationDto> StreamOrderUpdatesAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var response = await httpClient.GetAsync(
                "/orders/stream",
                HttpCompletionOption.ResponseHeadersRead, ct);

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (!ct.IsCancellationRequested)
            {
                string? line;
                try
                {
                    line = await reader.ReadLineAsync(ct);
                }
                catch (OperationCanceledException) { yield break; }
                catch (IOException) { yield break; }

                if (line == null) break;
                if (line.StartsWith("data: "))
                {
                    var json = line[6..];
                    var dto = JsonSerializer.Deserialize<OrderNotificationDto>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null) yield return dto;
                }
            }
        }
    }
}