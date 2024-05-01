using System.Runtime.CompilerServices;

namespace EasyCsv.Core;

public static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidIndex(int index, int size)
    {
        return index >= 0 && index < size;
    }
}