using Mongo.WebApi.Models;

namespace Mongo.WebApi.Services;
public interface IOrderService
{
    Task CancelOrderAsync(Guid id);
    Task CreateAsync(Order newOrder);
    Task<List<Order>> GetAsync();
    Task<Order> GetAsync(Guid id);
    Task RemoveAsync(Guid id);
    Task SetOrderPaidAsync(Guid id);
    Task UpdateAsync(Guid id, Order updatedOrder);
}