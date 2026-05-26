using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.DTOs;
using OrderManagement.Api.Models;
using OrderManagement.Api.Services;

namespace OrderManagement.Tests;

public class OrderServiceTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateOrder_WhenDataIsValid()
    {
        var context = GetInMemoryContext();
        var service = new OrderService(context);

        var dto = new CreateOrderDto
        {
            BuyerName = "João Teste",
            Products = new List<CreateProductDto>
            {
                new CreateProductDto { Name = "Produto 1", Price = 99.90m }
            }
        };

        var order = await service.CreateAsync(dto);

        order.Should().NotBeNull();
        order.Buyer.Name.Should().Be("João Teste");
        order.Products.Should().HaveCount(1);
        order.Status.Should().Be(OrderStatus.Iniciado);
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelOrder_WhenStatusIsIniciado()
    {
        var context = GetInMemoryContext();
        var service = new OrderService(context);

        var dto = new CreateOrderDto
        {
            BuyerName = "João Teste",
            Products = new List<CreateProductDto>
            {
                new CreateProductDto { Name = "Produto 1", Price = 99.90m }
            }
        };

        var order = await service.CreateAsync(dto);
        var cancelled = await service.CancelAsync(order.Id);

        cancelled.Status.Should().Be(OrderStatus.Cancelado);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenTransitionIsInvalid()
    {
        var context = GetInMemoryContext();
        var service = new OrderService(context);

        var dto = new CreateOrderDto
        {
            BuyerName = "João Teste",
            Products = new List<CreateProductDto>
            {
                new CreateProductDto { Name = "Produto 1", Price = 99.90m }
            }
        };

        var order = await service.CreateAsync(dto);

        var act = async () => await service.UpdateStatusAsync(order.Id, OrderStatus.Enviado);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}