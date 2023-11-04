using Domain;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

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

                               SELECT OrderId, OrderNumber, Customer.CustomerId FROM [Order]
                               WHERE CustomerId IN (SELECT CustomerId FROM @Customers);

                               SELECT OrderItemId, [Order].OrderId, Qty, PriceCharged,
                                      Product.ProductId, Product.[Name], Product.Price
                               FROM OrderItem
                               INNER JOIN[Order] ON[Order].OrderId = OrderItem.OrderId
                               INNER JOIN Product ON Product.ProductId = OrderItem.ProductId
                               WHERE CustomerId IN (SELECT CustomerId FROM @Customers); ";
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

        public async Task<IEnumerable<Customer>> GetByName(string searchName)
        {
            string sqlFilter = @"DECLARE @Customers TABLE (CustomerId INT PRIMARY KEY); 

                                 INSERT INTO @Customers (CustomerId)
                                 SELECT CustomerId FROM Customer
                                 WHERE [Name] LIKE @SearchName; ";

            object param = new { SearchName = searchName };
            return await Get(sqlFilter, param);
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
                    SetPropertyValue(() => o.Product, p);
                    return o;
                }, splitOn: "ProductId");

                foreach (var order in orders)
                {
                    SetBackingField(order, "_orderItems", orderItems.Where(_ => _.OrderId == order.OrderId).ToList());
                }

                foreach (var customer in customers)
                {
                    SetBackingField(customer, "_orders", orders.Where(_ => _.CustomerId == customer.CustomerId.Value).ToList());
                }
            }

            return customers;
        }
    }
}