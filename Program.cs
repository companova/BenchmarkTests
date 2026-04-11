using BenchmarkDotNet.Running;

Console.WriteLine("Start...");

// Run the Substring vs Slice Benchmark Test
_ = BenchmarkRunner.Run<Companova.BenchmarkTests.SubstringVsSlice>();

Console.Read();
