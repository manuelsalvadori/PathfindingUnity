using System.Runtime.CompilerServices;
using Unity.Mathematics;

public static class Heuristics
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int OctileDistance(int2 start, int2 end)
    {
        int dx = math.abs(start.x - end.x);
        int dy = math.abs(start.y - end.y);
        return 10 * (dx + dy) + (14 - 2 * 10) * math.min(dx, dy);
    }
}
