using BenchmarkDotNet.Attributes;

namespace Companova.BenchmarkTests
{
    /// <summary>
    /// This Benchmark test compares performans of Substring vs Slice.
    /// </summary>
	public class SubstringVsSlice
	{
        private string _testString = "Let's use BenchmarkDotNet";

        public SubstringVsSlice()
		{
		}

        [Benchmark]
        public void SpanNoToString()
        {
            // Just get a span (no ToString)
            ReadOnlySpan<char> span = _testString.AsSpan();

            // Also, Get Slices. But what would you do with them?
            ReadOnlySpan<char> subSpan1 = span.Slice(0, 2);
            ReadOnlySpan<char> subSpan2 = span.Slice(2, 9);

            // What would you do with Spans?
        }

        [Benchmark]
        public string Substring()
        {
            // Get Substrings and return one of them
            string substring1 = _testString.Substring(0, 2);
            string substring2 = _testString.Substring(2, 9);

            // You get Substrings and now you can do whatever you want with them as strings.
            return substring1;
        }

        [Benchmark]
        public string Span()
        {
            // Use Span to get a substring. 
            ReadOnlySpan<char> span1 = _testString.AsSpan(0, 2);
            ReadOnlySpan<char> span2 = _testString.AsSpan(2, 9);

            // Convert spans to strings, so we could something else in code with them.
            string substring1 = span1.ToString();
            string substring2 = span2.ToString();

            return substring1;
        }

        [Benchmark]
        public string SpanAndSlices()
        {
            // Get a span (no ToString)
            ReadOnlySpan<char> span = _testString.AsSpan();

            // The Slice out our strings
            ReadOnlySpan<char> subSpan1 = span.Slice(0, 2);
            ReadOnlySpan<char> subSpan2 = span.Slice(2, 9);

            // Convert to strings
            string substring1 = subSpan1.ToString();
            string substring2 = subSpan2.ToString();

            // Return a string
            return substring1;
        }

        [Benchmark]
        public void SpanAndSlicesNoReturn()
        {
            // Just get a span (no ToString)
            ReadOnlySpan<char> span = _testString.AsSpan();

            ReadOnlySpan<char> subSpan1 = span.Slice(0, 2);
            ReadOnlySpan<char> subSpan2 = span.Slice(2, 9);

            string substring1 = subSpan1.ToString();
            string substring2 = subSpan2.ToString();

            // Do not return a string.
        }
    }
}

