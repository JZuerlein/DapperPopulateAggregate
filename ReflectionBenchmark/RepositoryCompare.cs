using BenchmarkDotNet.Attributes;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReflectionBenchmark
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class RepositoryCompare
    {
        [Benchmark]
        public async Task GetWithSimpleRepository()
        {
            var repo = new SimpleCustomerRepository();
            var customers = await repo.GetByName("%");
        }

        [Benchmark]
        public async Task GetWithRepository()
        {
            var repo = new CustomerRepository();
            var customers = await repo.GetByName("%");
        }
    }
}
