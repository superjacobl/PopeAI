namespace PopeAI.Database.Annotations;

public class GuidID : ColumnAttribute
{
    public GuidID()
    {
        TypeName = "VARCHAR(36)";
    }
}

public class EntityId : ColumnAttribute
{
    public EntityId()
    {
        TypeName = "VARCHAR(38)";
    }
}

public class VarChar : ColumnAttribute
{
    public VarChar(int length)
    {
        TypeName = $"VARCHAR({length})";
    }
}