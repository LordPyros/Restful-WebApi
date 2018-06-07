namespace SupermarketWebApi.DTO
{
    public class SupermarketStockDTO
    {
        public int Id { get; set; }
        public int SupermarketId { get; set; }
        public int ProductId { get; set; }
        public int NumberInStock { get; set; }
    }
}
