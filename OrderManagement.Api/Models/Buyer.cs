namespace OrderManagement.Api.Models
{
    public class Buyer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }
}
