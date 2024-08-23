using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IVRC2024.Utils.Internals
{
    public static class RuntimeHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref byte GetRawData(object obj) => ref Unsafe.As<RawData>(obj).Data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(IntPtr pointer) => ref Unsafe.AddByteOffset(ref Unsafe.NullRef<T>(), pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetBaseSize(object obj)
        {
            ref var data = ref GetRawData(obj);
            ref var mt = ref AsRef<byte>(Unsafe.Subtract(ref Unsafe.As<byte, IntPtr>(ref data), 1));
            return Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref mt, 4));
        }

        [StructLayout(LayoutKind.Explicit)]
        private sealed class RawData
        {
            [FieldOffset(0)]
            public byte Data;
        }
    }
}
