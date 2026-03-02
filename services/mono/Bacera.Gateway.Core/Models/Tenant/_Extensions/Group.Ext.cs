namespace Bacera.Gateway;

using M = Group;

partial class Group
{
    public bool IsEmpty() => Id == 0;

    public static Group CreateAgentGroup(long ownerAccountId, string name, string description = "")
        => Build(AccountGroupTypes.Agent, ownerAccountId, name, description);

    public static Group CreateSalesGroup(long ownerAccountId, string name, string description = "")
        => Build(AccountGroupTypes.Sales, ownerAccountId, name, description);

    public static Group CreateRegGroup(long ownerAccountId, string name, string description = "")
        => Build(AccountGroupTypes.Rep, ownerAccountId, name, description);

    public static Group Build(AccountGroupTypes type, long ownerAccountId, string name, string description = "")
        => new()
        {
            Name = name,
            Type = (short)type,
            OwnerAccountId = ownerAccountId,
            Description = description,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };
}