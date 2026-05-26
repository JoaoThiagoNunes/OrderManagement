using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Api.DTOs;

public class CreateOrderDto
{

    [Required(ErrorMessage = "O comprador é obrigatório.")]
    public required string BuyerName { get; set; }

    [Required(ErrorMessage = "O pedido deve ter pelo menos um produto.")]
    [MinLength(1, ErrorMessage = "O pedido deve ter pelo menos um produto.")]
    public required List<CreateProductDto> Products { get; set; }
}

public class CreateProductDto
{
    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    public required string Name { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Price { get; set; }
}