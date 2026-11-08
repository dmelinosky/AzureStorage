// <copyright file="TableAccessIntegrationTests.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Integration tests for the <see cref="TableAccess{T}"/> class.
    /// </summary>
    [ClassDataSource<SetupStorageFixture>(Shared = SharedType.PerClass)]
    [NotInParallel]
    public sealed class TableAccessIntegrationTests : IDisposable
    {
        private TableAccess<TestEntity> sut;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAccessIntegrationTests"/> class.
        /// </summary>
        /// <param name="fixture">A storage fixture.</param>
        public TableAccessIntegrationTests(SetupStorageFixture fixture)
        {
            ArgumentNullException.ThrowIfNull(fixture);

            if (fixture.Table == null)
            {
                throw new InvalidOperationException("Fixture's Table property is null, have you set it up correctly?");
            }

            this.Fixture = fixture;

            this.sut = new TableAccess<TestEntity>(this.Fixture.Table);
        }

        /// <summary>
        /// Gets the storage fixture.
        /// </summary>
        public SetupStorageFixture Fixture { get; }

        /// <summary>
        /// Can get a single entity by partition and row key.
        /// </summary>
        /// <returns>A task.</returns>
        [Test]
        public async Task CanGetSingleSpecifiedEntity()
        {
            // Act
            TestEntity getIt = await this.sut.GetSingleAsync("test", "query1");

            // Assert
            await Assert.That(getIt).IsNotNull();
            await Assert.That(getIt.Name).IsEqualTo("Findable with get single");
        }

        /// <summary>
        /// Should be able to delete an entity without throwing.
        /// </summary>
        /// <returns>A task.</returns>
        [Test]
        public async Task CanDeleteSpecificEntity()
        {
            // Arrange
            TestEntity deletable = await this.sut.GetSingleAsync("test", "delete");

            // Act / Assert
            await Assert.That(() => this.sut.DeleteAsync(deletable)).ThrowsNothing();
            await Assert.That(() => this.sut.GetSingleAsync("test", "delete")).Throws<NotFoundException>();
        }

        /// <summary>
        /// Can insert an entity.
        /// </summary>
        /// <returns>A task.</returns>
        [Test]
        public async Task CanInsertWithoutThrowing()
        {
            // Arrange
            TestEntity insertable = new() { PartitionKey = "test", RowKey = "inserted", Name = "This was inserted" };

            // Act
            await Assert.That(() => this.sut.AddAsync(insertable)).ThrowsNothing();
            TestEntity verify = await this.sut.GetSingleAsync("test", "inserted");

            // Assert
            await Assert.That(verify).IsNotNull();
            await Assert.That(verify.Name).IsEqualTo("This was inserted");
        }

        /// <summary>
        /// Should be able to find via a property.
        /// </summary>
        /// <returns>A task.</returns>
        [Test]
        public async Task CanFindByProperty()
        {
            // Act
            TestEntity foundItem = await this.sut.FindFirstRowWithProperty("query1", "Name", "Findable with get single");

            // Assert
            await Assert.That(foundItem).IsNotNull();
            await Assert.That(foundItem.PartitionKey).IsEqualTo("test");
        }

        /// <summary>
        /// Can get a collection of items.
        /// </summary>
        /// <returns>A task.</returns>
        [Test]
        public async Task CanGetCollectionOfItems()
        {
            // Act
            IReadOnlyCollection<TestEntity> results = await this.sut.FindAllByPartitionKey("list");

            // Assert
            await Assert.That(results).IsNotNull();
            await Assert.That(results.Count).IsEqualTo(3);
        }

        /// <summary>
        /// Can get a collection of items with the same row key.
        /// </summary>
        /// <returns>A task.</returns>
        [Test]
        public async Task CanGetCollectionByRowKey()
        {
            // Act
            IReadOnlyCollection<TestEntity> results = await this.sut.FindAllByRowKey("row");

            // Assert
            await Assert.That(results).IsNotNull();
            await Assert.That(results.Count).IsEqualTo(2);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.sut?.Dispose();
            this.sut = null;
        }
    }
}
