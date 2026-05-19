using System.Collections.Concurrent;
using System.Net.ServerSentEvents;
using System.Threading.Channels;
using UTB.Minute.Contracts;
using UTB.Minute.Contracts.Orders;

namespace UTB.Minute.WebApi.Services
{
    public class OrderSseService
    {
        private readonly ConcurrentDictionary<Guid, Channel<SseItem<UpdateOrderStatusDto>>> clients = [];

        public async Task SendAsync(UpdateOrderStatusDto update)
        {
            foreach (var client in clients.Values)
            {
                await client.Writer.WriteAsync(new SseItem<UpdateOrderStatusDto>(update, "order"));
            }
        }
        public IAsyncEnumerable<SseItem<UpdateOrderStatusDto>>
            Stream(CancellationToken ct)
        {
            var id = Guid.NewGuid();
            var channel = Channel.CreateUnbounded<SseItem<UpdateOrderStatusDto>>();
            clients.TryAdd(id, channel);
            ct.Register(() => clients.TryRemove(id, out _));
            return channel.Reader.ReadAllAsync(ct);
        }
    }   
}
