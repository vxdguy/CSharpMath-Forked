using System;
using System.Collections.Generic;
using System.Text;
using MethodImpl = System.Runtime.CompilerServices.MethodImplAttribute;
using Impl = System.Runtime.CompilerServices.MethodImplOptions;

namespace CSharpMath {
  public static class ForEachExtensions {
    [MethodImpl(Impl.AggressiveInlining)]
    public static ForEach<T> AsForEach<T>(this IEnumerable<T> e) => new ForEach<T>(e);
    [MethodImpl(Impl.AggressiveInlining)]
    public static ForEach<T> AsForEach<T>(this T[] e) => new ForEach<T>(e);
    [MethodImpl(Impl.AggressiveInlining)]
    public static ForEach<T> AsForEach<T>(this T[] e, int start) => new ForEach<T>(e, start);
    [MethodImpl(Impl.AggressiveInlining)]
    public static ForEach<T> AsForEach<T>(this T[] e, int start, int length) => new ForEach<T>(e, start, length);
    [MethodImpl(Impl.AggressiveInlining)]
    public static ForEach<T> AsForEach<T>(this ArraySegment<T> e) => new ForEach<T>(e);
    [MethodImpl(Impl.AggressiveInlining)]
    public static ForEach<T> AsForEach<T>(this Span<T> e) => new ForEach<T>(e); //Span<T> -> ReadOnlySpan<T> is too long to be included explicitly
    [MethodImpl(Impl.AggressiveInlining)]
    public static ForEach<T> AsForEach<T>(this ReadOnlySpan<T> e) => new ForEach<T>(e);
  }

  public readonly ref struct ForEach<T> {
    [MethodImpl(Impl.AggressiveInlining)]
    public ForEach(IEnumerable<T> e) {
      enumerable = e;
      span = default;
    }

    [MethodImpl(Impl.AggressiveInlining)]
    public ForEach(ReadOnlySpan<T> r) {
      enumerable = default;
      span = r;
    }

    [MethodImpl(Impl.AggressiveInlining)]
    public ForEach(T[] a) {
      enumerable = default;
      span = new ReadOnlySpan<T>(a);
    }

    [MethodImpl(Impl.AggressiveInlining)]
    public ForEach(T[] a, int start) {
      enumerable = default;
      span = new ReadOnlySpan<T>(a, start, a.Length - start);
    }

    [MethodImpl(Impl.AggressiveInlining)]
    public ForEach(T[] a, int start, int length) {
      enumerable = default;
      span = new ReadOnlySpan<T>(a, start, length);
    }

    [MethodImpl(Impl.AggressiveInlining)]
    public ForEach(ArraySegment<T> s) {
      enumerable = default;
      span = new ReadOnlySpan<T>(s.Array, s.Offset, s.Count);
    }

    private readonly IEnumerable<T> enumerable;
    private readonly ReadOnlySpan<T> span;

    public static T[] AllocateNewArrayFor(ForEach<T> forEach) =>
      forEach.enumerable is null ? forEach.span.ToArray() : System.Linq.Enumerable.ToArray(forEach.enumerable);

    public void CopyTo(T[] array) {
      if (enumerable is null)
        span.CopyTo(array);
      else if (enumerable is ICollection<T> c)
        c.CopyTo(array, 0);
      else {
        int i = 0;
        foreach (var item in enumerable)
          array[i++] = item;
      }
    }
    public List<(T, TOther)> Zip<TOther>(ForEach<TOther> otherForEach, int sizeGuess = -1) {
      var list = sizeGuess is -1 ? new List<(T, TOther)>() : new List<(T, TOther)>(sizeGuess);
      var thisEnumerator = GetEnumerator();
      var thatEnumerator = otherForEach.GetEnumerator();
      try {
        while (thisEnumerator.MoveNext() && thatEnumerator.MoveNext())
          list.Add((thisEnumerator.Current, thatEnumerator.Current));
      } finally {
        thisEnumerator.Dispose();
        thatEnumerator.Dispose();
      }
      return list;
    }
    public override string ToString() => enumerable is null ? span.ToString() : enumerable.ToString();
    public Enumerator GetEnumerator() => new Enumerator(enumerable, span);

    public ref struct Enumerator {
      [MethodImpl(Impl.AggressiveInlining)]
      internal Enumerator(IEnumerable<T> enumerable, ReadOnlySpan<T> s) {
        enumerator = enumerable?.GetEnumerator();
        span = s;
        spanIndex = -1;
      }

      private readonly IEnumerator<T> enumerator;
      private int spanIndex;
      private readonly ReadOnlySpan<T> span;

      [MethodImpl(Impl.AggressiveInlining)]
      public bool MoveNext() => enumerator is null ? ++spanIndex < span.Length : enumerator.MoveNext();
      public T Current { [MethodImpl(Impl.AggressiveInlining)] get => enumerator is null ? span[spanIndex] : enumerator.Current; }
      [MethodImpl(Impl.AggressiveInlining)]
      public void Dispose() => enumerator?.Dispose();
      [MethodImpl(Impl.AggressiveInlining)]
      public void Reset() { if (enumerator is null) spanIndex = -1; else enumerator.Reset(); }
    }
  }
}
