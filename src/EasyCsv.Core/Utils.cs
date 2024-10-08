﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EasyCsv.Core;

internal static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidIndex(int index, int size)
    {
        return index >= 0 && index < size;
    }
    public static bool IsValidIndex<T>(this IReadOnlyList<T> collection, int index)
    {
        return index >= 0 && index < collection.Count;
    }

#if NETSTANDARD2_0
    public static bool IsValidIndex(int? index , int size)
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidIndex([NotNullWhen(true)] int? index, int size)
#endif
    {
        return index >= 0 && index < size;
    }
}