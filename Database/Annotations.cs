namespace PopeAI.Database.Annotations;

public class BigInt : ColumnAttribute
{
    public BigInt()
    {
        TypeName = "BIGINT";
    }
}

public class VarChar : ColumnAttribute
{
    public VarChar(int length)
    {
        TypeName = $"VARCHAR({length})";
    }
}

public class Text : ColumnAttribute
{
    public Text()
    {
        TypeName = $"TEXT";
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class IndexWithTypeAttribute : Attribute
{
    public string Name { get; set; }

    public IndexWithTypeAttribute(string name)
    {
        Name = name;
    }
}

public class DecimalType : ColumnAttribute
{
    public DecimalType(int precision = 2)
    {
        TypeName = $"NUMERIC(30, {precision})";
    }
}