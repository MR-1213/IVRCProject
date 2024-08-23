using IVRC2024.Utils.Internals;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace IVRC2024.Utils
{
    public ref struct SpanOwner<T>
    {
        private Span<T> _span;
        private ArrayPool<T>? _pool;
        private T[]? _array;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanOwner(Span<T> span)
        {
            _span = span;
            _pool = null;
            _array = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanOwner(int length) : this(length, ArrayPool<T>.Shared)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanOwner(int length, ArrayPool<T> pool)
        {
            var array = pool.Rent(length);
            _span = new(array, 0, length);
            _pool = pool;
            _array = array;
        }

        public readonly Span<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _pool?.Return(_array!);
            this = default;
        }
    }
}
