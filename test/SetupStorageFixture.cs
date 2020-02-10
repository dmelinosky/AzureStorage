// <copyright file="SetupStorageFixture.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
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
        public Task InitializeAsync()
        {
            string accountName = string.Empty;
            string secret = string.Empty;
            string tableName = string.Empty;

            StorageCredentials storageCredentials = new StorageCredentials(accountName, secret);
            CloudStorageAccount account = new CloudStorageAccount(storageCredentials, true);

            CloudTableClient client = account.CreateCloudTableClient();

            this.testTable = client.GetTableReference(tableName);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when an object is no longer needed. Called just before <see cref="System.IDisposable.Dispose" />
        /// if the class also implements that.
        /// </summary>
        /// <returns>A task.</returns>
        public Task DisposeAsync()
        {
            this.testTable = null;

            return Task.CompletedTask;
        }
    }
}
