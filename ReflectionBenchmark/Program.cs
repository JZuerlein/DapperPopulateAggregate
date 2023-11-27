// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using ReflectionBenchmark;

var summary = BenchmarkRunner.Run<RepositoryCompare>();

Console.ReadLine();
