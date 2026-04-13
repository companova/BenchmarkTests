# Introduction

`System.String` is one of the most frequently used reference types in .NET applications. It represents immutable sequences of UTF‑16 characters and underpins nearly all data processing, UI rendering, and inter‑component communication. Although `string` is a reference type allocated on the managed heap, it behaves like a value type in one important way: once created, its contents cannot be modified. Any operation that appears to “change” a string actually produces a new string instance.  

This combination—heap allocation plus immutability—makes efficient string handling critical for performance‑sensitive applications.  

This article examines the practical performance differences between `String.Substring` and `ReadOnlySpan<T>.Slice`, and why theoretical advantages of spans often disappear in real‑world code.  

# ReadOnlySpan<T> and Slice

`ReadOnlySpan<T>` is a stack‑only `ref struct` that provides a type‑safe, memory‑safe view over contiguous memory-managed or unmanaged. Because spans never escape to the heap, they avoid allocations entirely and offer excellent performance for slicing, parsing, and low‑level text processing.  

Key characteristics:
- **Stack‑only lifetime**: spans cannot be boxed.
- **No heap allocation**: slicing a span does not allocate memory. It's nothing more than pointer manipulation.
- **Restrictions**: spans cannot be used in async methods, stored as a class field or used as a type argument (List<Span<char>> cannot be used).
While spans excel in tight, synchronous, allocation‑free code paths, their stack-only nature imposes specific constraints that limit their applicability.

# Theory vs. Practice
Many developers are familiar with `Span<T>` and `ReadOnlySpan<T>` structures and their superior performance. I often hear an argument that it is better to use `Span.Slice()` method vs `String.Substring()` for string manipulations because slicing a span is allocation‑free. In isolation this is correct and the test results of this benchmark project confirm this.
However, in practice, I often see code that looks something like this:

```csharp
// Get Span
ReadOnlySpan<char> span = myString.AsSpan();
// Get a SubSpan/Slice
ReadOnlySpan<char> subSpan = span.Slice(2, 10);
// Convert span to String
string subString = subSpan.ToString();
```

Developers convert `span` back to `string` because they usually need to do something else with this data which spans do not allow. The final call to `ToString()` allocates a new `string` on the heap. At that point, the theoretical advantage of spans disappears. The code has performed extra work (creating spans and slicing them) only to end up with the same allocation that `Substring()` would have produced directly.

In other words: **if the end result must be a string, spans rarely provide a performance benefit.**

# Benchmark Scenarios
I setup an open-source benchmark project to compare five common substring‑extraction patterns from a .NET string:

| Test | Description |
|---|---|
| **SpanNoToString** | Creates a `ReadOnlySpan<char>` and slices without converting back to a string. Shows the raw cost of span operations alone.|
| **Substring** | Uses `string.Substring()`. Shows the raw cost of Substring operation which allocates new string objects on the heap.|
| **Span** | Calls `AsSpan()` with offset/length to get slices, then converts them to strings via `.ToString()`|
| **SpanAndSlices** | Gets a span of the entire string first, then uses `.Slice()` to carve out pieces, and converts them to strings.|
| **SpanAndSlicesNoReturn** | Same as above but doesn't return a value — useful for isolating the cost of the `.Slice()` + `.ToString()` work without a return overhead.|

# Benchmark Results
Environment: Apple M1 Max, 1 CPU, 10 logical and 10 physical cores .NET SDK 9.0.310.  
[Host] : .NET 8.0.23 (8.0.23, 8.0.2325.60607), Arm64 RyuJIT armv8.0-a

| Method                | Mean       | Error     | StdDev    |
|---------------------- |-----------:|----------:|----------:|
| SpanNoToString        |  0.2222 ns | 0.0030 ns | 0.0028 ns |
| Substring             |  **6.7718** ns | 0.0357 ns | 0.0316 ns |
| Span                  | **13.5729** ns | 0.0385 ns | 0.0321 ns |
| SpanAndSlices         | 13.9567 ns | 0.0748 ns | 0.0663 ns |
| SpanAndSlicesNoReturn | 13.7949 ns | 0.0953 ns | 0.0744 ns |


# Analysis
We can draw several conclusions from the data:  
### 1. Pure span slicing is the fastest operation
As expected, a direct call such as 
```csharp
ReadOnlySpan<char> subSpan2 = span.Slice(2, 9);
```
yields optimal performance. This is because `AsSpan()` and `Slice()` do not allocate new memory, though they are subject to ref struct restrictions.

### 2. If you need a string, `Substring()` is faster than span‑based approaches
Once the code calls `ToString()` on a `Span`, the cost roughly doubles compared to `Substring()`. This is because: `Substring()` is optimized for string memory layout.

### 3. Span‑based slicing introduces additional overhead before the allocation.
The simplest and fastest way to obtain a substring as a string remains:
```csharp
string substring = myString.Substring(2, 9);
```

### 4. Span‑based substring extraction only pays off when you avoid converting back to a string
If the downstream code can operate on spans, then spans are ideal. If not, the extra complexity and overhead provide no benefit.

#Conclusion
.NET provides multiple mechanisms for extracting substrings, but their performance characteristics differ significantly depending on the final goal.

* **If you can keep your data as a `ReadOnlySpan<char>`**, span slicing is by far the fastest and most allocation‑efficient option.
* **If you ultimately need a string**, `string.Substring()` is simpler, clearer, and faster than slicing spans and converting them back to strings.
* **Span‑based approaches only outperform `Substring` when no string allocation occurs.**

Understanding these trade‑offs helps ensure that performance‑oriented code actually delivers measurable benefits rather than adding unnecessary complexity.
