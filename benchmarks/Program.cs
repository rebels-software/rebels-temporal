using BenchmarkDotNet.Running;
using Rebels.Temporal.Benchmarks;

if (args.Length == 0)
{
    Console.WriteLine("Rebels.Temporal Benchmarks");
    Console.WriteLine("==========================");
    Console.WriteLine();
    Console.WriteLine("Available benchmarks:");
    Console.WriteLine("  1. PointMatchingSorted   - Point-to-point matching with sorted data (O(n+m))");
    Console.WriteLine("  2. PointMatchingUnsorted - Point-to-point matching with unsorted data (O(n×m))");
    Console.WriteLine("  3. Consumer              - Compare buffer implementation approaches");
    Console.WriteLine("  4. All                   - Run all benchmarks");
    Console.WriteLine();
    Console.Write("Select benchmark (1-4): ");

    var input = Console.ReadLine();

    args = input switch
    {
        "1" => ["--filter", "*PointMatchingSorted*"],
        "2" => ["--filter", "*PointMatchingUnsorted*"],
        "3" => ["--filter", "*Consumer*"],
        "4" => [],
        _ => ["--filter", "*PointMatching*"]
    };
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
