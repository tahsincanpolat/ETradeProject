namespace ETICARET.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Qunatity { get; set; }
    }
}