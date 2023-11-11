namespace Domain
{
    public class Order
    {
        private readonly List<OrderItem> _orderItems = new List<OrderItem>();
        public static readonly string OrderItemsFieldName = nameof(_orderItems);

        public Order() { }

        public int OrderId { get; protected set; }
        public int CustomerId { get; protected set; }
        public string OrderNumber { get; protected set; } = string.Empty;
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        public void AddOrderItem(OrderItem item)
        {
            _orderItems.Add(item);
        }
    }
}