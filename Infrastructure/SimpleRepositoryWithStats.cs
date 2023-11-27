using Dapper;
using Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class SimpleCustomerRepositoryWithStats
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

        public SimpleCustomerRepositoryWithStats()
        {
            SqlMapper.AddTypeHandler(new CustomerIdHandler());
        }

        public async Task<Customer?> GetByCustomerId(CustomerId customerId)
        {
            var (customers, customerBytes) = (await GetCustomers(customerId));
            var customer = customers.FirstOrDefault();

            if (customer == null) return null;

            var (orders, orderBytes) = await GetOrders(customerId);
            _setOrders.SetValue(customer, orders);

            foreach (var order in orders)
            {
                var orderItems = await GetOrderItems(order.OrderId);
                _setOrderItems.SetValue(order, orderItems);
            }

            return customer;
        }

        public async Task<IEnumerable<Customer>> GetByName(string searchName)
        {
            Int64 totalBytes = 0;

            var (customers, customerBytes) = await GetCustomers(searchName);
            totalBytes += customerBytes;

            foreach (var customer in customers)
            {
                var (orders, orderBytes) = await GetOrders(customer.CustomerId);
                totalBytes += orderBytes;


                _setOrders.SetValue(customer, orders.ToList());

                foreach (var order in customer.Orders)
                {
                    var (orderItems, itemBytes) = await GetOrderItems(order.OrderId);
                    totalBytes += itemBytes;
                    _setOrderItems.SetValue(order, orderItems.ToList());
                }
            }

            return customers;
        }

        private async Task<(IEnumerable<Customer>, Int64)> GetCustomers(CustomerId customerId)
        {
            const string sqlCustomer = @"SELECT CustomerId, [Name] FROM Customer
                                         WHERE CustomerId = @CustomerId";

            using (var connection = new SqlConnection(connStr))
            {
                connection.StatisticsEnabled = true;
                var result = await connection.QueryAsync<Customer>(sqlCustomer, new { CustomerId = customerId.Value });
                var currentStatistics = connection.RetrieveStatistics();
                return (result, (Int64)currentStatistics["BytesReceived"]);
            }
        }

        private async Task<(IEnumerable<Customer>, Int64)> GetCustomers(string searchName)
        {
            const string sqlCustomer = @"SELECT CustomerId, [Name] FROM Customer
                                         WHERE [Name] LIKE @SearchName;";

            using (var connection = new SqlConnection(connStr))
            {
                connection.StatisticsEnabled = true;
                var result = await connection.QueryAsync<Customer>(sqlCustomer, new { SearchName = searchName });
                var currentStatistics = connection.RetrieveStatistics();
                return (result, (Int64)currentStatistics["BytesReceived"]);
            }
        }

        private async Task<(IEnumerable<Order>, Int64)> GetOrders(CustomerId customerId)
        {
            const string sqlOrder = @"SELECT OrderId, OrderNumber, [Order].CustomerId FROM [Order]
                                      WHERE CustomerId = @CustomerId";

            using (var connection = new SqlConnection(connStr))
            {
                connection.StatisticsEnabled = true;
                var result = await connection.QueryAsync<Order>(sqlOrder, new { CustomerId = customerId.Value });
                var currentStatistics = connection.RetrieveStatistics();
                return (result, (Int64)currentStatistics["BytesReceived"]);
            }
        }

        private async Task<(IEnumerable<OrderItem>, Int64)> GetOrderItems(int orderId)
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

                var result = await connection.QueryAsync<OrderItem, Product, OrderItem>(sqlOrderItem, (o, p) =>
                {
                    _setProduct.SetValue(o, p);
                    return o;
                }, new { OrderId = orderId }, splitOn: "ProductId");

                var currentStatistics = connection.RetrieveStatistics();

                return (result, (Int64)currentStatistics["BytesReceived"]);
            }
        }
    }
}
