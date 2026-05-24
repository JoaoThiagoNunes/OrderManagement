using System.ComponentModel.DataAnnotations.Schema;

namespace OrderManagement.Api.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public Guid OrderId { get; set; }
}