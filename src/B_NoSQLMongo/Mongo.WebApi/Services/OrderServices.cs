using Microsoft.Extensions.Options;
using Mongo.WebApi.Models;
using Mongo.WebApi.Settings;
using MongoDB.Driver;

namespace Mongo.WebApi.Services;

public class OrderService : IOrderService
{
    private readonly IMongoCollection<Order> _ordersCollection;

    public OrderService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _ordersCollection = database.GetCollection<Order>(settings.Value.CollectionName);
    }

    public async Task<List<Order>> GetAsync() =>
        await _ordersCollection.Find(_ => true).ToListAsync();

    public async Task<Order> GetAsync(Guid id) =>
        await _ordersCollection.Find(order => order.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Order newOrder) =>
        await _ordersCollection.InsertOneAsync(newOrder);

    public async Task UpdateAsync(Guid id, Order updatedOrder) =>
        await _ordersCollection.ReplaceOneAsync(order => order.Id == id, updatedOrder);

    public async Task RemoveAsync(Guid id) =>
        await _ordersCollection.DeleteOneAsync(order => order.Id == id);

    public async Task CancelOrderAsync(Guid id)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.Id, id);
        var update = Builders<Order>.Update.Set(o => o.Status, "Cancelled");
        await _ordersCollection.UpdateOneAsync(filter, update);
    }

    public async Task SetOrderPaidAsync(Guid id)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.Id, id);
        var update = Builders<Order>.Update.Set(o => o.Status, "Paid");
        await _ordersCollection.UpdateOneAsync(filter, update);
    }
}