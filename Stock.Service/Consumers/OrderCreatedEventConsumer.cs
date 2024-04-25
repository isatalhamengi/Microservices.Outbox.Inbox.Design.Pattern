using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Stock.Service.Models.Contexts;
using Stock.Service.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Stock.Service.Consumers
{
    internal class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        StockDbContext _context;

        public OrderCreatedEventConsumer(StockDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var result = await _context.OrderInboxes.AnyAsync(i=> i.IdempotentToken == context.Message.IdempotentToken);
            if (!result)
            {
                await _context.OrderInboxes.AddAsync(new()
                {
                    Processed = false,
                    Payload = JsonSerializer.Serialize(context.Message),
                    IdempotentToken = context.Message.IdempotentToken   
                });
                await _context.SaveChangesAsync();
            }

            List<OrderInbox> orderInboxes = await _context.OrderInboxes.Where(x => x.Processed == false).ToListAsync();
            foreach (var orderInbox in orderInboxes)
            {
                OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderInbox.Payload);
                await Console.Out.WriteAsync($"{orderCreatedEvent.OrderId} order id değerine karşılık siparişin stok işlemleri başarıyla gerçekleşti.");
                orderInbox.Processed = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
