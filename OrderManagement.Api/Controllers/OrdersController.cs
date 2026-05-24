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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Products)
            .ToListAsync();

        return Ok(orders);
    }

   
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound("Pedido não encontrado.");

        if (order.Status != OrderStatus.Iniciado)
            return BadRequest("Apenas pedidos com status 'Iniciado' podem ser alterados.");

        if (dto.Products is null || dto.Products.Count == 0)
            return BadRequest("O pedido deve ter pelo menos um produto.");

        order.Buyer.Name = dto.BuyerName;

        var oldProducts = await _context.Products
            .Where(p => p.OrderId == order.Id)
            .ToListAsync();

        _context.Products.RemoveRange(oldProducts);

        var newProducts = dto.Products.Select(p => new Product
        {
            Id = Guid.NewGuid(),
            Name = p.Name,
            Price = p.Price,
            OrderId = order.Id
        }).ToList();

        await _context.Products.AddRangeAsync(newProducts);
        await _context.SaveChangesAsync();

        order.Products = newProducts;

        return Ok(order);
    }


    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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


