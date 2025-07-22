using Microsoft.AspNetCore.Mvc;
using Mongo.WebApi.Models;
using Mongo.WebApi.Services;

namespace Mongo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> Get() 
        => await _orderService.GetAsync();

    [HttpGet("{id:guid}", Name = "GetOrder")]
    public async Task<ActionResult<Order>> Get(Guid id)
    {
        var order = await _orderService.GetAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return order;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(Order newOrder)
    {
        if (newOrder.Items != null && newOrder.Items.Any())
        {
            newOrder.TotalAmount = newOrder.Items.Sum(item => item.Quantity * item.UnitPrice);
        }
        else
        {
            newOrder.TotalAmount = 0;
        }

        await _orderService.CreateAsync(newOrder);

        return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var order = await _orderService.GetAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        //if (order.Status != "Pending")
        //{
        //    return BadRequest("Only pending orders can be cancelled.");
        //}

        await _orderService.CancelOrderAsync(id);
        return NoContent(); // 204 No Content
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Order updatedOrder)
    {
        var existingOrder = await _orderService.GetAsync(id);

        if (existingOrder == null)
        {
            return NotFound();
        }

        updatedOrder.Id = existingOrder.Id;
        updatedOrder.CreatedAt = existingOrder.CreatedAt;

        if (updatedOrder.Items != null && updatedOrder.Items.Any())
        {
            updatedOrder.TotalAmount = updatedOrder.Items.Sum(item => item.Quantity * item.UnitPrice);
        }
        else
        {
            updatedOrder.TotalAmount = 0;
        }

        await _orderService.UpdateAsync(id, updatedOrder);

        return NoContent();
    }

    [HttpPut("{id:guid}/set-paid")]
    public async Task<IActionResult> SetPaid(Guid id)
    {
        var order = await _orderService.GetAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        await _orderService.SetOrderPaidAsync(id);
        return NoContent();
    }
}