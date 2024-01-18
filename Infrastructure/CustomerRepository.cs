using Domain;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Infrastructure
{
    public class CustomerIdHandler : SqlMapper.TypeHandler<CustomerId>
    {
        public override CustomerId Parse(object value)
        {
            return new CustomerId((int)value);
        }

        public override void SetValue(IDbDataParameter parameter, CustomerId value)
        {
            parameter.Value = value.Value;
        }
    }

    public class CustomerRepository : RepositoryBase
    {
        const string connStr = "Server=localhost;Database=AggregateDemo;Trusted_Connection=True;TrustServerCertificate=True";

        const string sqlBase = @"SELECT CustomerId, [Name] FROM Customer
                               WHERE CustomerId IN (SELECT CustomerId FROM @Customers);

                               SELECT OrderId, OrderNumber, [Order].CustomerId FROM [Order]
                               WHERE CustomerId IN (SELECT CustomerId FROM @Customers);

                               SELECT OrderItemId, [Order].OrderId, Qty, PriceCharged,
                                      Product.ProductId, Product.Price, Product.[Name]
                               FROM OrderItem
                               INNER JOIN[Order] ON[Order].OrderId = OrderItem.OrderId
                               INNER JOIN Product ON Product.ProductId = OrderItem.ProductId
                               WHERE CustomerId IN (SELECT CustomerId FROM @Customers); ";

        #region PropertyInfo and FieldInfo

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

        public CustomerRepository()
        {
            SqlMapper.AddTypeHandler(new CustomerIdHandler());
        }

        public async Task<IEnumerable<Customer>> GetByCustomerId(CustomerId customerId)
        {
            string sqlFilter = @"DECLARE @Customers TABLE (CustomerId INT PRIMARY KEY); 

                                 INSERT INTO @Customers (CustomerId)
                                 SELECT CustomerId FROM Customer
                                 WHERE CustomerId = @CustomerId; ";

            object param = new { CustomerId = customerId.Value };
            return await Get(sqlFilter, param);
        }

        public async Task<IEnumerable<Customer>> GetByCustomerIdWithSortedSpans(CustomerId customerId)
        {
            string sqlFilter = @"DECLARE @Customers TABLE (CustomerId INT PRIMARY KEY); 

                                 INSERT INTO @Customers (CustomerId)
                                 SELECT CustomerId FROM Customer
                                 WHERE CustomerId = @CustomerId; ";

            object param = new { CustomerId = customerId.Value };
            return await GetWithSortedSpans(sqlFilter, param);
        }

        public async Task<IEnumerable<Customer>> GetByName(string searchName)
        {
            string sqlFilter = @"DECLARE @Customers TABLE (CustomerId INT PRIMARY KEY); 

                                 INSERT INTO @Customers (CustomerId)
                                 SELECT CustomerId FROM Customer
                                 WHERE [Name] LIKE @SearchName; ";

            object param = new { SearchName = searchName };
            return await Get(sqlFilter, param);
        }

        public async Task<IEnumerable<Customer>> GetByNameWithSortedSpans(string searchName)
        {
            string sqlFilter = @"DECLARE @Customers TABLE (CustomerId INT PRIMARY KEY); 

                                 INSERT INTO @Customers (CustomerId)
                                 SELECT CustomerId FROM Customer
                                 WHERE [Name] LIKE @SearchName; ";

            object param = new { SearchName = searchName };
            return await GetWithSortedSpans(sqlFilter, param);
        }

        private async Task<IEnumerable<Customer>> Get(string sqlFilter, object param)
        {
            IEnumerable<Customer> customers;

            using (var connection = new SqlConnection(connStr))
            {
                using var grid = await connection.QueryMultipleAsync(sqlFilter + sqlBase, param);

                customers = grid.Read<Customer>();
                var orders = grid.Read<Order>();

                var orderItems = grid.Read<OrderItem, Product, OrderItem>((o, p) =>
                {
                    _setProduct.SetValue(o, p);
                    return o;

                }, splitOn: "ProductId");

                foreach (var order in orders)
                {
                    _setOrderItems.SetValue(order, orderItems.Where(_ => _.OrderId == order.OrderId).ToList());
                }

                foreach (var customer in customers)
                {
                    _setOrders.SetValue(customer, orders.Where(_ => _.CustomerId == customer.CustomerId.Value).ToList());
                }
            }

            return customers;
        }

        private async Task<IEnumerable<Customer>> GetWithSortedSpans(string sqlFilter, object param)
        {
            IEnumerable<Customer> customers;

            using (var connection = new SqlConnection(connStr))
            {
                using var grid = await connection.QueryMultipleAsync(sqlFilter + sqlBase, param);

                customers = grid.Read<Customer>();
                var orders = grid.Read<Order>();

                var orderItems = grid.Read<OrderItem, Product, OrderItem>((o, p) =>
                {
                    _setProduct.SetValue(o, p);
                    return o;
                }, splitOn: "ProductId");

                return BuildCustomerAggregates(customers.ToList(), orders.ToList(), orderItems.ToList());
            }
        }

        private IEnumerable<Customer> BuildCustomerAggregates(List<Customer> c,
                                                              List<Order> o,
                                                              List<OrderItem> oi)
        {
            var sortOrderItemsByOrderId = new SortOrderItemsByOrderId();
            var sortOrdersByCustomerId = new SortOrdersByCustomerId();

            var customers = CollectionsMarshal.AsSpan<Customer>(c);
            var orders = CollectionsMarshal.AsSpan<Order>(o);
            var orderItems = CollectionsMarshal.AsSpan<OrderItem>(oi);

            orders.Sort();
            orderItems.Sort(sortOrderItemsByOrderId);

            int start = 0;
            int length = 0;
            for (var i = 0; i < orders.Length; i++)
            {
                length = 0;
                while (start + length < orderItems.Length && orderItems[start + length].OrderId == orders[i].OrderId)
                {
                    length++;
                }
                _setOrderItems.SetValue(orders[i], new List<OrderItem>(orderItems.Slice(start, length).ToArray()));
                start += length;
            }

            orders.Sort(sortOrdersByCustomerId);
            customers.Sort();

            start = 0;
            for (var i = 0; i < customers.Length; i++)
            {
                length = 0;
                while (start + length < orders.Length && orders[start + length].CustomerId == customers[i].CustomerId.Value)
                {
                    length++;
                }
                _setOrders.SetValue(customers[i], new List<Order>(orders.Slice(start, length).ToArray()));
                start += length;
            }

            return customers.ToArray();
        }
    }
}