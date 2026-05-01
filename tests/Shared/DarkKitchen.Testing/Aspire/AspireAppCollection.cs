namespace DarkKitchen.Testing.Aspire;

[CollectionDefinition(Name)]
public sealed class AspireAppCollection : ICollectionFixture<AspireAppFixture>
{
    public const string Name = "Aspire application";
}
