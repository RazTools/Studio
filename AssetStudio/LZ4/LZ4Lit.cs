using System;

namespace AssetStudio;
public class LZ4Lit : LZ4
{
    public new static LZ4Lit Instance => new();
    protected override (int encCount, int litCount) GetLiteralToken(ReadOnlySpan<byte> cmp, ref int cmpPos) => ((cmp[cmpPos] >> 4) & 0xf, (cmp[cmpPos++] >> 0) & 0xf);
}