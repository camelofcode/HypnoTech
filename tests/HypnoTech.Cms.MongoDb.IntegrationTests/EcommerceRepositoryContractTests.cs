using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.MongoDb.Repositories;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Ecommerce.Dal.MongoDb.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

public sealed partial class CmsRepositoryContractTests
{
    [Fact]
    public async Task Ecommerce_async_repositories_preserve_catalogue_postage_order_and_download_code_contracts()
    {
        RegisterEcommerceMappings();

        await ForEachEcommerceTargetAsync(async options =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var database = Database(options);
            await database.GetCollection<BsonDocument>("countries").InsertOneAsync(
                new Country
                {
                    Code = "GB",
                    DisplayText = "United Kingdom",
                    Region = "UK"
                }.ToBsonDocument(),
                cancellationToken: cancellationToken);

            var categoryTypes = new MerchCategoryTypeRepository(options);
            var merch = new MerchRepository(options, categoryTypes);
            await database.GetCollection<BsonDocument>("merchcategorytypes").InsertOneAsync(
                new MerchCategoryType
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = "Shop",
                    Order = 1,
                    SubcategoryTypes =
                    [
                        new MerchSubcategoryType { Name = "Prints", Order = 1 },
                        new MerchSubcategoryType { Name = "Audio", Order = 2 }
                    ]
                }.ToBsonDocument(),
                cancellationToken: cancellationToken);

            await database.GetCollection<BsonDocument>("merches").InsertManyAsync(
                [
                    new MerchItem
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        Name = "Synthetic audio",
                        Category = "Shop",
                        SubCategory = "Audio",
                        Price = "6.50",
                        PostageProfile = "Used profile",
                        Slug = "synthetic-audio",
                        Options = []
                    }.ToBsonDocument(),
                    new MerchItem
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        Name = "Synthetic print",
                        Category = "Shop",
                        SubCategory = "Prints",
                        Price = "14.00",
                        PostageProfile = "Used profile",
                        Slug = "synthetic-print",
                        Options = []
                    }.ToBsonDocument()
                ],
                cancellationToken: cancellationToken);

            var flattened = await merch.GetMerchFlattenedAsync(cancellationToken);
            var grouped = await merch.GetMerchAsync(cancellationToken);
            Assert.Equal(["Synthetic print", "Synthetic audio"], flattened.Select(item => item.Name));
            Assert.Equal("Synthetic print", grouped.Single().SubCategories.First().Items.Single().Name);
            Assert.Equal("Synthetic audio", (await merch.GetMerchItemAsync(
                "synthetic-audio",
                cancellationToken)).Name);

            var postage = new PostageProfileRepository(options, merch);
            var usedProfile = CreatePostageProfile("Used profile", 3.50m);
            var unusedProfile = CreatePostageProfile("Unused profile", 1.25m);
            await postage.AddProfileAsync(usedProfile, cancellationToken);
            await postage.AddProfileAsync(unusedProfile, cancellationToken);

            var profiles = await postage.GetProfilesAsync(cancellationToken);
            Assert.False(profiles.Single(profile => profile.Name == "Used profile").CanDelete);
            Assert.True(profiles.Single(profile => profile.Name == "Unused profile").CanDelete);
            Assert.Contains("cannot be removed", await postage.RemoveProfileAsync(
                usedProfile.Id,
                cancellationToken));
            Assert.Null(await postage.RemoveProfileAsync(unusedProfile.Id, cancellationToken));

            usedProfile.UnitedKingdom = new PostageRegion { Alone = 4.25m, WithOthers = 1.50m };
            await postage.UpdateProfileAsync(usedProfile, cancellationToken);
            Assert.Equal(4.25m, (await postage.GetProfileAsync(
                "Used profile",
                cancellationToken)).UnitedKingdom.Alone);

            var orders = new OrderRepository(options, new CountryRepository(options));
            var completedOrder = CreateOrder("PAY-COMPLETE");
            await orders.SaveOrderAsync(completedOrder, cancellationToken);
            Assert.Equal("United Kingdom", (await orders.GetOrderAsync(
                completedOrder.PaymentId,
                cancellationToken)).Address.Country);

            var completed = await orders.CompleteOrderAsync(
                new PaymentCompletion
                {
                    PaymentId = completedOrder.PaymentId,
                    PayerId = "synthetic-payer",
                    SaleId = "synthetic-sale",
                    SaleUrl = "https://example.test/sale/synthetic-sale",
                    EmailAddress = "buyer@example.test"
                },
                cancellationToken);
            Assert.Equal("COMPLETE", completed.Status);
            Assert.Single((await orders.GetOrdersAsync(
                new PagedRequest { Page = 1, PageSize = 10 },
                cancellationToken)).Items);
            Assert.Equal("SHIPPED", (await orders.MarkAsShippedAsync(
                completedOrder.PaymentId,
                cancellationToken)).Status);

            var cancelledOrder = CreateOrder("PAY-CANCELLED");
            await orders.SaveOrderAsync(cancelledOrder, cancellationToken);
            await orders.CancelOrderAsync(cancelledOrder.PaymentId, cancellationToken);
            Assert.Equal("CANCELLED", (await orders.GetOrderAsync(
                cancelledOrder.PaymentId,
                cancellationToken)).Status);

            var downloadCodes = new DownloadCodeRepository(options);
            var collection = new DownloadCodeCollection
            {
                Name = "Synthetic album",
                RedemptionUrl = "https://example.test/redeem",
                Codes =
                [
                    new DownloadCode { Code = "SYNTHETIC-ONE" },
                    new DownloadCode { Code = "SYNTHETIC-TWO" }
                ]
            };
            await downloadCodes.AddCodeCollectionAsync(collection, cancellationToken);
            Assert.Equal("SYNTHETIC-ONE", await downloadCodes.AssignDownloadCodeAsync(
                collection.Id,
                "first@example.test",
                cancellationToken));

            var assignment = await downloadCodes.AssignDownloadCodesAsync(
                new Order
                {
                    EmailAddress = "second@example.test",
                    DownloadCodeIds = [collection.Id, collection.Id]
                },
                cancellationToken);
            Assert.Equal("SYNTHETIC-TWO", Assert.Single(assignment.AssignedDownloadCodes).Code);
            Assert.Equal("Synthetic album", Assert.Single(assignment.ExpiredCollections));

            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                categoryTypes.GetTypesAsync(cancelled.Token));
        });
    }

    private async Task ForEachEcommerceTargetAsync(Func<IOptions<MongoOptions>, Task> assertion)
    {
        foreach (var target in _fixture.Targets)
        {
            var options = Options.Create(new MongoOptions
            {
                Server = target.Host,
                Port = target.Port,
                Database = $"ht_e_{target.Name.Replace('-', '_').Replace('.', '_')}_{Guid.NewGuid():N}"
            });

            try
            {
                await assertion(options);
            }
            catch (Exception exception)
            {
                throw new Xunit.Sdk.XunitException($"MongoDB target '{target.Name}' failed: {exception}");
            }
            finally
            {
                await new MongoClient(options.Value.ConnectionString)
                    .DropDatabaseAsync(options.Value.Database);
            }
        }
    }

    private static void RegisterEcommerceMappings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Mongo:Server"] = "localhost",
                ["Mongo:Port"] = "27017",
                ["Mongo:Database"] = "hypnotech_ecommerce_mapping_tests"
            })
            .Build();
        var services = new ServiceCollection();
        HypnoTech.Common.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForCommon(
            services,
            configuration);
        HypnoTech.Ecommerce.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForEcommerce(
            services,
            configuration);
    }

    private static PostageProfile CreatePostageProfile(string name, decimal alone)
    {
        return new PostageProfile
        {
            Name = name,
            UnitedKingdom = new PostageRegion { Alone = alone, WithOthers = 1m },
            EuropeanUnion = new PostageRegion { Alone = alone, WithOthers = 1m },
            RestOfWorld1 = new PostageRegion { Alone = alone, WithOthers = 1m },
            RestOfWorld2 = new PostageRegion { Alone = alone, WithOthers = 1m },
            RestOfWorld3 = new PostageRegion { Alone = alone, WithOthers = 1m }
        };
    }

    private static Order CreateOrder(string paymentId)
    {
        return new Order
        {
            DateTime = new DateTime(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc),
            PaymentId = paymentId,
            Status = "CREATED",
            Address = new PostageAddress { CountryCode = "GB" },
            Items = [],
            DownloadCodeIds = []
        };
    }
}
