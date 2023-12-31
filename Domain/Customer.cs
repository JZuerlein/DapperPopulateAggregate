﻿
namespace Domain
{
    public class Customer
    {
        private List<Order> _orders = new List<Order>();
        public Customer() { }

        public const string OrdersFieldName = nameof(_orders);

        public CustomerId CustomerId { get; protected set; }
        public bool IsDeceased { get; protected set; }
        public string Name { get; protected set; } = string.Empty;
        public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
        public void AddOrder(Order order)
        {
            if (IsDeceased) throw new Exception("Not allowed.");

            _orders.Add(order); 
        }
    }
}
