using OrderManagement.Api.App.DTOs;
using OrderManagement.Api.DTOs;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetAllAsync(OrderFilterDto filter);
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(CreateOrderDto dto);
    Task<Order> UpdateAsync(Guid id, UpdateOrderDto dto);
    Task<Order> CancelAsync(Guid id);
    Task<Order> UpdateStatusAsync(Guid id, OrderStatus newStatus);
}