using System;
using System.Runtime.CompilerServices;

namespace Intense.Internal
{
    internal static class Error
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowArgumentNullException<T>(T value, string paramName) where T : class
        {
            if (value == null) ThrowArgumentNullExceptionCore(paramName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentNullExceptionCore(string paramName) => throw new ArgumentNullException(paramName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfInvalid(bool isValid, string message)
        {
            if (!isValid) ThrowInvalidOperationExceptionCore(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationExceptionCore(string message) => throw new InvalidOperationException(message);
    }
}