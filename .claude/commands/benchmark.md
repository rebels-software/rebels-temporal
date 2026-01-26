# Run Performance Benchmarks

Run BenchmarkDotNet performance benchmarks for the Rebels.Temporal library.

## Your Task

Execute benchmarks based on the user's request and report the results.

## Instructions

1. **Parse the filter argument** (if provided):
   - No argument or empty: Run all benchmarks
   - `sorted` or `Sorted`: Filter for sorted matching benchmarks
   - `unsorted` or `Unsorted`: Filter for unsorted matching benchmarks
   - `point` or `Point`: Filter for point matching benchmarks
   - `interval` or `Interval`: Filter for interval matching benchmarks
   - Any other value: Use as BenchmarkDotNet filter pattern

2. **Build and run benchmarks:**
   ```bash
   cd benchmarks
   dotnet run -c Release -- --filter *<pattern>*
   ```

3. **Report results** including:
   - Mean execution time
   - Memory allocations
   - Comparison between different approaches (if applicable)

## Available Benchmarks

| Benchmark Class | Description |
|-----------------|-------------|
| `PointMatchingSortedBenchmarks` | O(n+m) dual-pointer algorithm for sorted data |
| `PointMatchingUnsortedBenchmarks` | O(n×m) nested loops for unsorted data |

## Example Commands

```bash
# Run all benchmarks
cd benchmarks && dotnet run -c Release

# Run only sorted benchmarks
cd benchmarks && dotnet run -c Release -- --filter *Sorted*

# Run specific benchmark class
cd benchmarks && dotnet run -c Release -- --filter *PointMatchingSorted*
```

## Expected Performance

Reference results (2,000 anchors x 2,000 candidates):

| InputOrdering | Algorithm | Expected Time | Complexity |
|---------------|-----------|---------------|------------|
| `Both` | Dual-pointer | ~56 μs | O(n+m) |
| `None` | Nested loops | ~14 ms | O(n×m) |

Sorted data should be approximately **255x faster** than unsorted.

## Filter Argument

$ARGUMENTS
