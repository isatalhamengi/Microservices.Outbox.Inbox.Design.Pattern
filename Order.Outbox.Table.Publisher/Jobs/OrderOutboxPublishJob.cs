using MassTransit;
using Order.Outbox.Table.Publisher.Entities;
using Quartz;
using Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Order.Outbox.Table.Publisher.Jobs
{
    public class OrderOutboxPublishJob : IJob
    {
        IPublishEndpoint _publishEndpoint;

        public OrderOutboxPublishJob(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (OrderOutboxSingeltonDatabase.DataReaderState)
            {
                List<OrderOutbox> orderOutboxes = (await OrderOutboxSingeltonDatabase.QueryAsync<OrderOutbox>($@"Select * from OrderOutboxes Where ProcessedDate is NULL Order by OccuredOn asc")).ToList();

                foreach (var orderOutbox in orderOutboxes)
                {
                    if (orderOutbox.Type == nameof(OrderCreatedEvent))
                    {
                        OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderOutbox.Payload);
                        if (orderCreatedEvent != null)
                        {
                            await _publishEndpoint.Publish(orderCreatedEvent);
                            OrderOutboxSingeltonDatabase.ExecuteAsync($"Update OrderOutboxes Set ProcessedDate = GetDate() Where Id ='{orderOutbox.Id}'");
                        }
                    }
                }

                OrderOutboxSingeltonDatabase.DataReaderReady();
                await Console.Out.WriteLineAsync("Order Outbox Table Checked!");
            }
        }
    }
}
