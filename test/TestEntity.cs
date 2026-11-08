// <copyright file="TestEntity.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage.Tests
{
    using System;
    using Azure;
    using Azure.Data.Tables;

    /// <summary>
    /// An entity to put into table storage to test with.
    /// </summary>
    public class TestEntity : ITableEntity
    {
        /// <inheritdoc/>
        public string PartitionKey { get; set; }

        /// <inheritdoc/>
        public string RowKey { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset? Timestamp { get; set; }

        /// <inheritdoc/>
        public ETag ETag { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        public string Name { get; set; }
    }
}
