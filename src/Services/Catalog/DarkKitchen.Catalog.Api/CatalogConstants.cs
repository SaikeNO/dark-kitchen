namespace DarkKitchen.Catalog.Api;

public static class CatalogRoles
{
    public const string Manager = "Manager";
    public const string Operator = "Operator";
}

public static class CatalogPolicies
{
    public const string Manager = "catalog.manager";
    public const string Operator = "catalog.operator";
}

public static class CatalogDemoAccounts
{
    public const string ManagerEmail = "manager@darkkitchen.local";
    public const string OperatorEmail = "operator@darkkitchen.local";
    public const string Password = "Demo123!";
}
