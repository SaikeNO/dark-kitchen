using System.Reflection;
using System.Text.Json;
using DarkKitchen.Contracts.Events;
using JasperFx.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.RabbitMQ.Internal;

namespace DarkKitchen.ServiceDefaults;

public static class IntegrationEventBusExtensions
{
    private const string RabbitMqConnectionName = "rabbitmq";

    public static WebApplicationBuilder AddIntegrationEventBus(
        this WebApplicationBuilder builder,
        string postgresConnectionName,
        DarkKitchenService service)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(postgresConnectionName))
        {
            throw new ArgumentException("PostgreSQL connection name is required.", nameof(postgresConnectionName));
        }

        var postgresConnectionString = RequiredConnectionString(builder.Configuration, postgresConnectionName);
        var rabbitMqConnectionString = RequiredConnectionString(builder.Configuration, RabbitMqConnectionName);
        var subscription = IntegrationEventTopology.SubscriptionFor(service);

        builder.Host.UseWolverine(options =>
        {
            options.ApplicationAssembly = Assembly.GetEntryAssembly() ?? typeof(IntegrationEventBusExtensions).Assembly;

            options.PersistMessagesWithPostgresql(postgresConnectionString);
            options.UseSystemTextJsonForSerialization(ConfigureJson);

            var rabbitMq = options.UseRabbitMq(new Uri(rabbitMqConnectionString))
                .AutoProvision();

            DeclareEventExchanges(rabbitMq);
            ConfigureEventPublishers(options);

            if (subscription.EventTypes.Count > 0)
            {
                options.ListenToRabbitQueue(subscription.QueueName)
                    .UseDurableInbox();
            }

            options.Policies.UseDurableInboxOnAllListeners();
            options.Policies.UseDurableOutboxOnAllSendingEndpoints();
            options.Policies.AutoApplyIdempotencyOnNonTransactionalHandlers();
            options.Durability.KeepAfterMessageHandling = TimeSpan.FromMinutes(30);

            options.Services.AddResourceSetupOnStartup(StartupAction.SetupOnly);
        });

        return builder;
    }

    private static string RequiredConnectionString(IConfiguration configuration, string name)
    {
        return configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Missing required connection string '{name}'.");
    }

    private static void ConfigureJson(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = IntegrationEventJson.SerializerOptions.PropertyNamingPolicy;
        options.PropertyNameCaseInsensitive = IntegrationEventJson.SerializerOptions.PropertyNameCaseInsensitive;
        options.WriteIndented = IntegrationEventJson.SerializerOptions.WriteIndented;
    }

    private static void DeclareEventExchanges(RabbitMqTransportExpression rabbitMq)
    {
        foreach (var contract in KnownIntegrationEventContracts.All)
        {
            var exchangeName = IntegrationEventTopology.ExchangeFor(contract.EventType);
            var queues = IntegrationEventTopology.QueuesFor(contract.EventType);

            rabbitMq.DeclareExchange(exchangeName, exchange =>
            {
                exchange.ExchangeType = ExchangeType.Fanout;

                foreach (var queueName in queues)
                {
                    exchange.BindQueue(queueName);
                }
            });
        }
    }

    private static void ConfigureEventPublishers(WolverineOptions options)
    {
        options.PublishMessage<IntegrationEventEnvelope<OrderPlaced>>()
            .ToRabbitExchange(IntegrationEventTopology.ExchangeFor(KnownIntegrationEventTypes.OrderPlaced))
            .UseDurableOutbox();

        options.PublishMessage<IntegrationEventEnvelope<InventoryReserved>>()
            .ToRabbitExchange(IntegrationEventTopology.ExchangeFor(KnownIntegrationEventTypes.InventoryReserved))
            .UseDurableOutbox();

        options.PublishMessage<IntegrationEventEnvelope<InventoryReservationFailed>>()
            .ToRabbitExchange(IntegrationEventTopology.ExchangeFor(KnownIntegrationEventTypes.InventoryReservationFailed))
            .UseDurableOutbox();

        options.PublishMessage<IntegrationEventEnvelope<OrderAccepted>>()
            .ToRabbitExchange(IntegrationEventTopology.ExchangeFor(KnownIntegrationEventTypes.OrderAccepted))
            .UseDurableOutbox();

        options.PublishMessage<IntegrationEventEnvelope<ItemPreparationStarted>>()
            .ToRabbitExchange(IntegrationEventTopology.ExchangeFor(KnownIntegrationEventTypes.ItemPreparationStarted))
            .UseDurableOutbox();

        options.PublishMessage<IntegrationEventEnvelope<ItemPreparationCompleted>>()
            .ToRabbitExchange(IntegrationEventTopology.ExchangeFor(KnownIntegrationEventTypes.ItemPreparationCompleted))
            .UseDurableOutbox();

        options.PublishMessage<IntegrationEventEnvelope<OrderReadyForPacking>>()
            .ToRabbitExchange(IntegrationEventTopology.ExchangeFor(KnownIntegrationEventTypes.OrderReadyForPacking))
            .UseDurableOutbox();

        options.PublishMessage<IntegrationEventEnvelope<OrderReadyForPickup>>()
            .ToRabbitExchange(IntegrationEventTopology.ExchangeFor(KnownIntegrationEventTypes.OrderReadyForPickup))
            .UseDurableOutbox();
    }
}
