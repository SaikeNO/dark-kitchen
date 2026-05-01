using DarkKitchen.Testing.Aspire;

namespace DarkKitchen.AppHost.IntegrationTests;

[CollectionDefinition("Aspire application")]
public sealed class AspireAppTestCollection : ICollectionFixture<AspireAppFixture>;
