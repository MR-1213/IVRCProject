using IVRC2024.Utils.Internals;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace IVRC2024.Utils
{
    public struct RentedArray<T> : IMemoryOwner<T>
    {
        private readonly T[] _array;
        private readonly int _length;
        private ArrayPool<T>? _pool;

        public RentedArray(T[] array, int length, ArrayPool<T>? pool)
        {
            _array = array;
            _length = length;
            _pool = pool;
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        public readonly Span<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pool is not null ? new(_array, 0, _length) : throw new ObjectDisposedException(nameof(RentedArray<T>));
        }

        public readonly Memory<T> Memory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pool is not null ? new(_array, 0, _length) : throw new ObjectDisposedException(nameof(RentedArray<T>));
        }

        public readonly ArraySegment<T> Segment
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pool is not null ? new(_array, 0, _length) : throw new ObjectDisposedException(nameof(RentedArray<T>));
        }

        public readonly ref T this[int index]
        {
            get
            {
                if (_pool is null)
                {
                    throw new ObjectDisposedException(nameof(RentedArray<T>));
                }
                if ((uint)index >= (uint)_length)
                {
                    throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.");
                }
                return ref _array[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RentedArray<T> Rent(int length) => Rent(length, ArrayPool<T>.Shared);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RentedArray<T> Rent(int length, ArrayPool<T> pool) => new(pool.Rent(length), length, pool);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T>.Enumerator GetEnumerator() => _pool is not null ? Span.GetEnumerator() : throw new ObjectDisposedException(nameof(RentedArray<T>));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_pool is not null)
            {
                _pool.Return(_array);
                _pool = null;
            }
        }
    }
}
