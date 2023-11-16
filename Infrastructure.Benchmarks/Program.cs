// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using Infrastructure.Benchmarks;

var results = BenchmarkRunner.Run<BenchmarkTests>();

Console.ReadLine();
