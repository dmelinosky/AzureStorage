// <copyright file="SetupStorageFixture.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage.Tests
{
    using System;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using Microsoft.Extensions.Configuration;
    using TUnit.Core.Interfaces;

    /// <summary>
    /// A fixture which sets up a storage table for integration tests.
    /// </summary>
    /// <remarks>
    /// Storage is resolved from user secrets or environment variables, in order:
    /// <c>StorageConnectionString</c>; <c>AccountName</c> + <c>AccountKey</c>;
    /// otherwise the local Azurite emulator (<c>UseDevelopmentStorage=true</c>).
    /// </remarks>
    public class SetupStorageFixture : IAsyncInitializer, IAsyncDisposable
    {
        /// <summary>
        /// The name of the seeded test table.
        /// </summary>
        public const string TableName = "test";

        /// <summary>
        /// Gets the table the fixture is setting up for testing.
        /// </summary>
        public TableClient Table { get; private set; }

        /// <summary>
        /// Gets the table service client for the storage account under test.
        /// </summary>
        public TableServiceClient ServiceClient { get; private set; }

        /// <summary>
        /// Called before the fixture is used by any test.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task InitializeAsync()
        {
            Microsoft.Extensions.Configuration.IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets(typeof(SetupStorageFixture).Assembly, optional: true)
                .AddEnvironmentVariables()
                .Build();

            this.ServiceClient = CreateServiceClient(config);

            this.Table = this.ServiceClient.GetTableClient(TableName);

            await this.Table.DeleteAsync();
            await this.Table.CreateIfNotExistsAsync();

            TestEntity[] seed =
            [
                new() { PartitionKey = "test", RowKey = "query1", Name = "Findable with get single" },
                new() { PartitionKey = "test", RowKey = "delete", Name = "Here to be deleted." },
                new() { PartitionKey = "list", RowKey = "one", Name = "list one" },
                new() { PartitionKey = "list", RowKey = "two", Name = "list two" },
                new() { PartitionKey = "list", RowKey = "three", Name = "list three" },
                new() { PartitionKey = "hello", RowKey = "row", Name = "row one" },
                new() { PartitionKey = "there", RowKey = "row", Name = "row two" },
            ];

            foreach (TestEntity entity in seed)
            {
                await this.Table.AddEntityAsync(entity);
            }
        }

        /// <summary>
        /// Deletes the test table once the fixture is no longer needed.
        /// </summary>
        /// <returns>A task.</returns>
        public async ValueTask DisposeAsync()
        {
            if (this.Table != null)
            {
                await this.Table.DeleteAsync();
                this.Table = null;
            }

            GC.SuppressFinalize(this);
        }

        private static TableServiceClient CreateServiceClient(Microsoft.Extensions.Configuration.IConfiguration config)
        {
            string connectionString = config["StorageConnectionString"];

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return new TableServiceClient(connectionString);
            }

            string accountName = config["AccountName"];
            string accountKey = config["AccountKey"];

            if (!string.IsNullOrWhiteSpace(accountName) && !string.IsNullOrWhiteSpace(accountKey))
            {
                return new TableServiceClient(
                    new Uri($"https://{accountName}.table.core.windows.net"),
                    new TableSharedKeyCredential(accountName, accountKey));
            }

            return new TableServiceClient("UseDevelopmentStorage=true");
        }
    }
}
