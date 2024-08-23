using System;
using System.Runtime.CompilerServices;

namespace IVRC2024.Utils.Internals
{
    public static class ArrayMarshal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayDataReference<T>(T[] array)
        {
            return ref Unsafe.As<byte, T>(ref Unsafe.AddByteOffset(ref RuntimeHelpers.GetRawData(array), (IntPtr)Unsafe.SizeOf<int>()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayDataReference<T>(T[,] array) => ref GetArrayDataReference<T>((Array)array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayDataReference<T>(T[,,] array) => ref GetArrayDataReference<T>((Array)array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayDataReference<T>(Array array)
        {
            ref var data = ref RuntimeHelpers.GetRawData(array);
            var offset = RuntimeHelpers.GetBaseSize(array) - (nuint)(2 * Unsafe.SizeOf<IntPtr>());
            return ref Unsafe.As<byte, T>(ref Unsafe.AddByteOffset(ref data, offset));
        }
    }
}
