using Infrastructure;

var repository = new CustomerRepository();
var simpleRepository = new SimpleCustomerRepositoryWithStats();
//var customers = await simpleRepository.GetByName("%");

var customers = await repository.GetByName("%");

foreach (var customer in customers)
{
    Console.WriteLine(@"CustomerId = {0}", customer.CustomerId);
    Console.WriteLine(customer.Name);
    foreach(var order in customer.Orders)
    {
        Console.WriteLine(order.OrderNumber);

        foreach(var orderItem in order.OrderItems)
        {
            Console.WriteLine(@"OrderItemId {0}, ProductName {1}, Qty {2}, PriceCharged {3}",
                orderItem.OrderItemId, orderItem.Product.Name, orderItem.Qty, orderItem.PriceCharged);
        }
    }

    Console.WriteLine("---------------");
}

Console.ReadLine();
