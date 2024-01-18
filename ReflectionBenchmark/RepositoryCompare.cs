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
        public async Task GetByNameWithLinqWhere()
        {
            var repo = new CustomerRepository();
            var customers = await repo.GetByName("Name_2222%");
        }

        [Benchmark]
        public async Task GetByNameWithSortedSpans()
        {
            var repo = new CustomerRepository();
            var customers = await repo.GetByNameWithSortedSpans("Name_2222%");
        }

        [Benchmark]
        public async Task GetByIdWithLinqWhere()
        {
            var repo = new CustomerRepository();
            var customers = await repo.GetByCustomerId(new Domain.CustomerId(2222));
        }

        [Benchmark]
        public async Task GetByIdWithSortedSpans()
        {
            var repo = new CustomerRepository();
            var customers = await repo.GetByCustomerIdWithSortedSpans(new Domain.CustomerId(2222));
        }
    }
}
