using Dapper;
using Domain;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace Infrastructure
{
    public class SimpleCustomerRepository
    {
        const string connStr = "Server=localhost;Database=AggregateDemo;Trusted_Connection=True;TrustServerCertificate=True";

        #region Reflection

        private readonly PropertyInfo _setProduct = typeof(OrderItem)
            .GetProperty("Product", BindingFlags.NonPublic |
                            BindingFlags.Instance |
                            BindingFlags.Public)!;

        private readonly FieldInfo _setOrderItems = typeof(Order)
            .GetField("_orderItems", BindingFlags.NonPublic |
                                     BindingFlags.Instance)!;

        public readonly FieldInfo _setOrders = typeof(Customer)
            .GetField(Customer.OrdersFieldName, BindingFlags.NonPublic |
                                 BindingFlags.Instance)!;

        #endregion

        public SimpleCustomerRepository() 
        {
            SqlMapper.AddTypeHandler(new CustomerIdHandler());
        }

        public async Task<Customer?> GetByCustomerId(CustomerId customerId)
        {
            var customer = (await GetCustomers(customerId)).FirstOrDefault();

            if (customer == null) return null;

            var orders = await GetOrders(customerId);
            _setOrders.SetValue(customer, orders);

            foreach(var order in orders)
            {
                var orderItems = await GetOrderItems(order.OrderId);
                _setOrderItems.SetValue(order, orderItems);
            }

            return customer;
        }

        public async Task<IEnumerable<Customer>> GetByName(string searchName)
        {
            var customers = await GetCustomers(searchName);

            foreach(var customer in customers)
            {
                var orders= await GetOrders(customer.CustomerId);
                _setOrders.SetValue(customer, orders.ToList());

                foreach (var order in customer.Orders)
                {
                    var orderItems = await GetOrderItems(order.OrderId);
                    _setOrderItems.SetValue(order, orderItems.ToList());
                }
            }

            return customers;
        }

        private async Task<IEnumerable<Customer>> GetCustomers(CustomerId customerId)
        {
            const string sqlCustomer = @"SELECT CustomerId, [Name] FROM Customer
                                         WHERE CustomerId = @CustomerId";

            using (var connection = new SqlConnection(connStr))
            {
                return await connection.QueryAsync<Customer>(sqlCustomer, new { CustomerId = customerId.Value });
            }
        }

        private async Task<IEnumerable<Customer>> GetCustomers(string searchName)
        {
            const string sqlCustomer = @"SELECT CustomerId, [Name] FROM Customer
                                         WHERE [Name] LIKE @SearchName;";

            using (var connection = new SqlConnection(connStr))
            {
                return await connection.QueryAsync<Customer>(sqlCustomer, new { SearchName = searchName });
            }
        }

        private async Task<IEnumerable<Order>> GetOrders(CustomerId customerId)
        {
            const string sqlOrder = @"SELECT OrderId, OrderNumber, [Order].CustomerId FROM [Order]
                                      WHERE CustomerId = @CustomerId";

            using (var connection = new SqlConnection(connStr))
            {
                return await connection.QueryAsync<Order>(sqlOrder, new { CustomerId = customerId.Value});
            }
        }

        private async Task<IEnumerable<OrderItem>> GetOrderItems(int orderId)
        {
            string sqlOrderItem = @"SELECT OrderItemId, [Order].OrderId, Qty, PriceCharged,
                                      Product.ProductId, Product.[Name], Product.Price
                                    FROM OrderItem
                                    INNER JOIN[Order] ON[Order].OrderId = OrderItem.OrderId
                                    INNER JOIN Product ON Product.ProductId = OrderItem.ProductId
                                    WHERE OrderItem.OrderId = @OrderId;";

            using (var connection = new SqlConnection(connStr))
            {
                connection.StatisticsEnabled = true;

                return await connection.QueryAsync<OrderItem, Product, OrderItem>(sqlOrderItem, (o, p) =>
                {
                    _setProduct.SetValue(o, p);
                    return o;
                }, new { OrderId = orderId }, splitOn: "ProductId");
            }
        }
    }
}
