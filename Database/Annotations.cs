namespace PopeAI.Database.Annotations;

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