using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;
using OrderManagement.Api.DTOs;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Products)
            .ToListAsync();

        return Ok(orders);
    }

   
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound("Pedido não encontrado.");

        return Ok(order);
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var buyer = new Buyer
        {
            Id = Guid.NewGuid(),
            Name = dto.BuyerName
        };

        var products = dto.Products.Select(p => new Product
        {
            Id = Guid.NewGuid(),
            Name = p.Name,
            Price = p.Price
        }).ToList();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Iniciado,
            CreatedAt = DateTime.UtcNow,
            Buyer = buyer,
            BuyerId = buyer.Id,
            Products = products
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Order updatedOrder)
    {
        var order = await _context.Orders
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound("Pedido não encontrado.");

        if (order.Status != OrderStatus.Iniciado)
            return BadRequest("Apenas pedidos com status 'Iniciado' podem ser alterados.");

        if (updatedOrder.Products is null || updatedOrder.Products.Count == 0)
            return BadRequest("O pedido deve ter pelo menos um produto.");

        order.Products = updatedOrder.Products;
        order.BuyerId = updatedOrder.BuyerId;

        await _context.SaveChangesAsync();

        return Ok(order);
    }


    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus newStatus)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order is null)
            return NotFound("Pedido não encontrado.");

        var valid = (order.Status, newStatus) switch
        {
            (OrderStatus.Iniciado, OrderStatus.Processado) => true,
            (OrderStatus.Iniciado, OrderStatus.Cancelado) => true,
            (OrderStatus.Processado, OrderStatus.Enviado) => true,
            (OrderStatus.Processado, OrderStatus.Cancelado) => true,
            _ => false
        };

        if (!valid)
            return BadRequest($"Não é possível mudar o status de '{order.Status}' para '{newStatus}'.");

        order.Status = newStatus;
        await _context.SaveChangesAsync();

        return Ok(order);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order is null)
            return NotFound("Pedido não encontrado.");

        if (order.Status != OrderStatus.Iniciado && order.Status != OrderStatus.Processado)
            return BadRequest("Apenas pedidos 'Iniciados' ou 'Processados' podem ser cancelados.");

        order.Status = OrderStatus.Cancelado;
        await _context.SaveChangesAsync();

        return Ok(order);
    }

}


