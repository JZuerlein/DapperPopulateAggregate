﻿// See https://aka.ms/new-console-template for more information
using Infrastructure;

var repository = new CustomerRepository();
var customers = await repository.GetByName("%j%");

foreach (var customer in customers)
{
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