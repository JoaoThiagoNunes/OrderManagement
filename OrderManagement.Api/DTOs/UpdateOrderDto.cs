using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Api.DTOs;

public class UpdateOrderDto
{
    [Required(ErrorMessage = "O nome do comprador é obrigatório.")]
    public required string BuyerName { get; set; }

    [Required(ErrorMessage = "O pedido deve ter pelo menos um produto.")]
    [MinLength(1, ErrorMessage = "O pedido deve ter pelo menos um produto.")]
    public required List<CreateProductDto> Products { get; set; }
}

