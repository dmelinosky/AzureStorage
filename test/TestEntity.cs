// <copyright file="TestEntity.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage.Tests
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// An entity to put into table storage to test with.
    /// </summary>
    public class TestEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        public string Name { get; set; }
    }
}
