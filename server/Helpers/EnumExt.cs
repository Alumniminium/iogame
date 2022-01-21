using System.Runtime.CompilerServices;

namespace server.Helpers
{
    public static class EnumExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe bool HasFlags<T>(T* first, T* second) where T : unmanaged, Enum
        {
            var pf = (byte*)first;
            var ps = (byte*)second;

            for (var i = 0; i < sizeof(T); i++)
                if ((pf[i] & ps[i]) != ps[i])
                    return false;

            return true;
        }

        /// <remarks>Faster analog of Enum.HasFlag</remarks>
        /// <inheritdoc cref="Enum.HasFlag"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool HasFlags<T>(this T first, T second) where T : unmanaged, Enum => HasFlags(&first, &second);
    }
}