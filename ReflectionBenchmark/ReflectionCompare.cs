using BenchmarkDotNet.Attributes;
using Domain;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReflectionBenchmark
{
    //public class ReflectionCompare : CustomerRepository
    //{
    //    private readonly Customer customer = new Customer();
    //    private readonly Order order = new Order();
    //    private readonly List<Order> orders = new List<Order>();

    //    [Benchmark]
    //    public void SetWithReflection()
    //    {
    //        _setOrders.SetValue(customer, orders);
    //    }

    //    [Benchmark]
    //    public void SetWithExpression()
    //    {
    //        SetPropertyValue(() => customer._protectedOrders, orders);
    //    }

    //    [Benchmark]
    //    public void SetWithPublic()
    //    {
    //        customer._publicOrders = orders;
    //    }
    //}
}
