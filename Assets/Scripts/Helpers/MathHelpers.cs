using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

// Handy tips and tricks for using DOTS math:
// https://discussions.unity.com/t/unity-mathematics-tips-and-tricks/897200/9

// these functions are for where the math library fails us ...

[BurstCompile]
public static class MathHelpers
{
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MoveTowards(ref float3 current, ref float3 target, float maxDistanceDelta, out float3 result)
    {
        float dirX = target.x - current.x;
        float dirY = target.y - current.y;
        float dirZ = target.z - current.z;

        float sqrLength = dirX * dirX + dirY * dirY + dirZ * dirZ;

        if (sqrLength == 0.0 || maxDistanceDelta >= 0.0 && sqrLength <= maxDistanceDelta * maxDistanceDelta)
        {
            result = target;
            return;
        }

        float dist = math.sqrt(sqrLength);

        result = new float3(current.x + dirX / dist * maxDistanceDelta,
                          current.y + dirY / dist * maxDistanceDelta,
                          current.z + dirZ / dist * maxDistanceDelta);
    }

    // A tiny floating point value (RO).
    //public static readonly float Epsilon =
    //    UnityEngineInternal.MathfInternal.IsFlushToZeroEnabled ? UnityEngineInternal.MathfInternal.FloatMinNormal
    //    : UnityEngineInternal.MathfInternal.FloatMinDenormal;

    //// Compares two floating point values if they are similar.
    ////https://discussions.unity.com/t/unity-mathematics-equivalent-to-mathf-approximately-for-float2-float3/882086/2
    //[BurstCompile]
    //public static bool3 Approximately(float3 a, float3 b)
    //{
    //    // If a or b is zero, compare that the other is less or equal to epsilon.
    //    // If neither a or b are 0, then find an epsilon that is good for
    //    // comparing numbers at the maximum magnitude of a and b.
    //    // Floating points have about 7 significant digits, so
    //    // 1.000001f can be represented while 1.0000001f is rounded to zero,
    //    // thus we could use an epsilon of 0.000001f for comparing values close to 1.
    //    // We multiply this epsilon by the biggest magnitude of a and b.
    //    return math.abs(b - a) < math.max(0.000001f * math.max(math.abs(a), math.abs(b)), Epsilon * 8);
    //}
}