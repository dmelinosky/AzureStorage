// <copyright file="SetupStorageFixture.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    /// <summary>
    /// A fixture which sets up a storage account for integration tests.
    /// </summary>
    public class SetupStorageFixture : IAsyncLifetime
    {
        private CloudTable testTable;

        /// <summary>
        /// Gets the table the fixture is setting up for testing.
        /// </summary>
        public CloudTable Table => this.testTable;

        /// <summary>
        /// Called immediately after the class has been created, before it is used.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task InitializeAsync()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly());

            var config = builder.Build();

            // these need to be read from elsewhere.
            string accountName = config["AccountName"];
            string secret = config["AccountKey"];
            string tableName = "test";

            StorageCredentials storageCredentials = new StorageCredentials(accountName, secret);
            CloudStorageAccount account = new CloudStorageAccount(storageCredentials, true);

            CloudTableClient client = account.CreateCloudTableClient();

            this.testTable = client.GetTableReference(tableName);

            await this.testTable.DeleteIfExistsAsync();

            await this.testTable.CreateIfNotExistsAsync();

            TestEntity findableEntity = new TestEntity { PartitionKey = "test", RowKey = "query1", Name = "Findable with get single" };

            TableOperation insertOp = TableOperation.Insert(findableEntity);

            await this.testTable.ExecuteAsync(insertOp);

            TestEntity deletableEntity = new TestEntity { PartitionKey = "test", RowKey = "delete", Name = "Here to be deleted." };

            insertOp = TableOperation.Insert(deletableEntity);

            await this.testTable.ExecuteAsync(insertOp);
        }

        /// <summary>
        /// Called when an object is no longer needed. Called just before <see cref="System.IDisposable.Dispose" />
        /// if the class also implements that.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task DisposeAsync()
        {
            await this.testTable.DeleteIfExistsAsync();

            this.testTable = null;
        }
    }
}
