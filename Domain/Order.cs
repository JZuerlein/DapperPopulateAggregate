namespace Domain
{
    public class Order : IComparable<Order>
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

        public int CompareTo(Order? other)
        {
            if (this.OrderId < other.OrderId) return -1;
            if (this.OrderId == other.OrderId) return 0;
            return 1;
        }
    }

    public class SortOrdersByCustomerId : IComparer<Order>
    {
        public int Compare(Order x, Order y)
        {
            return x.CustomerId.CompareTo(y.CustomerId);
        }
    }
}