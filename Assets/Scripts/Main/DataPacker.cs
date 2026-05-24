using Unity.Mathematics;

public static class DataPacker
{
    public static long Int3PackLong(int3 value)
    {
        long packed = 0;
        packed |= (value.x & 0x3FFFL) << 0; // 14位
        packed |= (value.y & 0xFFFFFFL) << 14; // 24位
        packed |= (value.z & 0xFFFFFL) << 38; // 20位
        return packed;
    }

    public static int3 LongUnpackInt3(long packed)
    {
        var x = (int)((packed >> 0) & 0x3FFFL); // 14位掩码
        var y = (int)((packed >> 14) & 0xFFFFFFL); // 24位掩码
        var z = (int)((packed >> 38) & 0xFFFFFL); // 20位掩码
        return new int3(x, y, z);
    }

    public static int Int2PackInt(int2 value)
    {
        var packed = 0;
        packed |= (value.x & 0xFFFFF) << 0; // 20位掩码 (0xFFFFF = 1048575)
        packed |= (value.y & 0x7F) << 20; // 7位掩码 (0x7F = 127)
        return packed;
    }

    public static int2 IntUnpackInt2(int packed)
    {
        var x = (packed >> 0) & 0xFFFFF; // 20位
        var y = (packed >> 20) & 0x7F; // 7位
        return new int2(x, y);
    }
}
