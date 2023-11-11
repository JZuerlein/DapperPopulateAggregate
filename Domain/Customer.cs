
using System.Reflection;

namespace Domain
{
    public class Customer
    {
        private readonly List<Order> _orders = new List<Order>();
        public static readonly string OrdersFieldName = nameof(_orders);

        public Customer() { }
        public CustomerId CustomerId { get; protected set; }
        public string Name { get; protected set; } = string.Empty;
        public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
    }
}
