using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models.Contexts;
using Order.API.Models.Entities;
using Order.API.ViewModels;
using Shared;
using Shared.Events;
using System.Text.Json;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderControllers : ControllerBase
    {
        OrderDbContext _context;
        ISendEndpointProvider _sendEndpointProvider;

        public OrderControllers(OrderDbContext context, ISendEndpointProvider sendEndpointProvider)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder(CreateOrderVM createOrderVM)
        {
            try
            {
                Order.API.Models.Entities.Order order = new()
                {
                    BuyerId = createOrderVM.BuyerId,
                    CreatedDate = DateTime.UtcNow,
                    OrderItems = createOrderVM.OrderItems.Select(oi => new Order.API.Models.Entities.OrderItem
                    {
                        Price = oi.Price,
                        Count = oi.Count,
                        ProductId = oi.ProductId
                    }).ToList(),
                    TotalPrice = createOrderVM.OrderItems.Sum(oi => oi.Price * oi.Count)
                };

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                var idempotentToken = Guid.NewGuid();
                OrderCreatedEvent orderCreatedEvent = new()
                {
                    BuyerId = order.BuyerId,
                    OrderId = order.Id,
                    OrderItems = createOrderVM.OrderItems.Select(x => new Shared.Datas.OrderItem
                    {
                        Count = x.Count,
                        Price = x.Price,
                        ProductId = x.ProductId
                    }).ToList(),
                    IdempotentToken = idempotentToken
                };

                #region Outbox Pattern Olmadan
                //var sendEndPoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEvent}"));
                //await sendEndPoint.Send<OrderCreatedEvent>(orderCreatedEvent);
                //return Ok(orderCreatedEvent);
                #endregion
                #region Outbox Pattern
                OrderOutbox orderOutbox = new()
                {
                    OccuredOn = DateTime.UtcNow,
                    ProcessedDate = null,
                    Payload = JsonSerializer.Serialize(orderCreatedEvent),
                    //Type = orderCreatedEvent.GetType().Name
                    Type = nameof(OrderCreatedEvent),
                    IdempotentToken = idempotentToken
                };

                await _context.OrderOutboxes.AddAsync(orderOutbox);
                await _context.SaveChangesAsync();
                return Ok(orderCreatedEvent);
                #endregion
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
