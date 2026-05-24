using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.DTOs;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Products)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> CreateAsync(CreateOrderDto dto)
    {
        var buyer = new Buyer
        {
            Id = Guid.NewGuid(),
            Name = dto.BuyerName
        };

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Iniciado,
            CreatedAt = DateTime.UtcNow,
            Buyer = buyer,
            BuyerId = buyer.Id,
            Products = dto.Products.Select(p => new Product
            {
                Id = Guid.NewGuid(),
                Name = p.Name,
                Price = p.Price
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<Order> UpdateAsync(Guid id, UpdateOrderDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        if (order.Status != OrderStatus.Iniciado)
            throw new InvalidOperationException("Apenas pedidos com status 'Iniciado' podem ser alterados.");

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

        return order;
    }

    public async Task<Order> CancelAsync(Guid id)
    {
        var order = await _context.Orders.FindAsync(id)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        if (order.Status != OrderStatus.Iniciado && order.Status != OrderStatus.Processado)
            throw new InvalidOperationException("Apenas pedidos 'Iniciados' ou 'Processados' podem ser cancelados.");

        order.Status = OrderStatus.Cancelado;
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<Order> UpdateStatusAsync(Guid id, OrderStatus newStatus)
    {
        var order = await _context.Orders.FindAsync(id)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        var valid = (order.Status, newStatus) switch
        {
            (OrderStatus.Iniciado, OrderStatus.Processado) => true,
            (OrderStatus.Iniciado, OrderStatus.Cancelado) => true,
            (OrderStatus.Processado, OrderStatus.Enviado) => true,
            (OrderStatus.Processado, OrderStatus.Cancelado) => true,
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException($"Não é possível mudar o status de '{order.Status}' para '{newStatus}'.");

        order.Status = newStatus;
        await _context.SaveChangesAsync();

        return order;
    }
}