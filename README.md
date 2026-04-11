# BenchmarkTests

**BenchmarkTests** is an open-source .NET 8.0 console application that uses the [BenchmarkDotNet](https://benchmarkdotnet.org/) library to run performance benchmarks comparing different .NET Framework APIs and coding approaches. Its goal is to provide reliable performance data so developers can understand the runtime costs and trade-offs of various .NET types and methods.

## Current Benchmark: `Substring` vs `Span`/`Slice`

The repo currently contains one benchmark class — **`SubstringVsSlice`** — which measures and compares five different ways to extract substrings from a .NET `string`:

| Method | What it does |
|---|---|
| **`SpanNoToString`** | Creates a `ReadOnlySpan<char>` and slices it, but never converts back to a `string`. Shows the raw cost of span operations alone. |
| **`Substring`** | Uses the classic `string.Substring()` method, which allocates new `string` objects on the heap. |
| **`Span`** | Calls `AsSpan()` with offset/length to get slices, then converts them back to strings via `.ToString()`. |
| **`SpanAndSlices`** | Gets a span of the entire string first, then uses `.Slice()` to carve out pieces, and converts them to strings. |
| **`SpanAndSlicesNoReturn`** | Same as above but doesn't return a value — useful for isolating the cost of the slice + ToString work without a return overhead. |

## How It Works

- `Program.cs` is the entry point; it calls `BenchmarkRunner.Run<SubstringVsSlice>()` to execute the benchmarks.
- BenchmarkDotNet handles warm-up, multiple iterations, statistical analysis, and result reporting automatically.
- The project uses strict code analysis (`AnalysisLevel=latest`, `AnalysisMode=All`, Roslyn analyzers enabled).

## Project Structure

```
BenchmarkTests/
├── Program.cs                  # Entry point — runs the benchmark suite
├── Benchmarks/
│   └── SubstringVsSlice.cs     # Benchmark class comparing Substring vs Span/Slice
├── BenchmarkTests.csproj       # .NET 8 project, references BenchmarkDotNet 0.15.8
├── GlobalSuppressions.cs       # Code analysis suppressions
└── README.md
```

## Running the Benchmarks

Build and run the project in **Release** mode (required by BenchmarkDotNet):

```bash
dotnet run -c Release
```

BenchmarkDotNet will output a detailed summary table with mean execution time, memory allocations, and statistical information for each benchmark method.

## License

This project is open-source. See the [LICENSE](LICENSE) file for details.
