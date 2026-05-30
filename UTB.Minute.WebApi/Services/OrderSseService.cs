using System.Collections.Concurrent;
using System.Net.ServerSentEvents;
using System.Threading.Channels;
using UTB.Minute.Contracts;
using UTB.Minute.Contracts.Orders;

namespace UTB.Minute.WebApi.Services
{
    public class OrderSseService
    {
        private readonly ConcurrentDictionary<Guid, Channel<SseItem<OrderNotificationDto>>> _clients = [];

        public async Task SendAsync(OrderNotificationDto notification)
        {
            foreach (var client in _clients.Values)
                await client.Writer.WriteAsync(new SseItem<OrderNotificationDto>(notification, "order"));
        }

        public IAsyncEnumerable<SseItem<OrderNotificationDto>> Stream(CancellationToken ct)
        {
            var id = Guid.NewGuid();
            var channel = Channel.CreateUnbounded<SseItem<OrderNotificationDto>>();
            _clients.TryAdd(id, channel);
            ct.Register(() => _clients.TryRemove(id, out _));
            return channel.Reader.ReadAllAsync(ct);
        }
    }
}
