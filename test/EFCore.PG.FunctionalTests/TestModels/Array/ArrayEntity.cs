using System.Collections.Generic;

#nullable enable

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Array;

public class ArrayEntity
{
    public int Id { get; set; }
    public int[] IntArray { get; set; } = null!;
    public List<int> IntList { get; set; } = null!;
    public int?[] NullableIntArray { get; set; } = null!;
    public List<int?> NullableIntList { get; set; } = null!;
    public byte[] Bytea { get; set; } = null!;
    public byte[] ByteArray { get; set; } = null!;
    public string[] StringArray { get; set; } = null!;
    public List<string> StringList { get; set; } = null!;
    public string?[] NullableStringArray { get; set; } = null!;
    public List<string?> NullableStringList { get; set; } = null!;
    public string? NullableText { get; set; }
    public string NonNullableText { get; set; } = null!;
    public SomeEnum ValueConvertedScalar { get; set; }
    public SomeEnum[] ValueConvertedArray { get; set; } = null!;
    public List<SomeEnum> ValueConvertedList { get; set; } = null!;
    public byte Byte { get; set; }
}