// <copyright file="TableAccessIntegrationTests.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage.Tests
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Integration tests for the <see cref="Gobie74.AzureStorage.TableAccess{T}"/> class.
    /// </summary>
    public partial class TableAccessIntegrationTests : IClassFixture<SetupStorageFixture>, IDisposable
    {
        private TableAccess<TestEntity> sut;
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAccessIntegrationTests"/> class.
        /// </summary>
        /// <param name="fixture">A storage fixture.</param>
        public TableAccessIntegrationTests(SetupStorageFixture fixture)
        {
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture));
            }

            if (fixture.Table == null)
            {
                throw new Exception("Fixture's Table property is null, have you set it up correctly?");
            }

            this.Fixture = fixture;

            this.sut = new TableAccess<TestEntity>(this.Fixture.Table);
        }

        /// <summary>
        /// Gets the storage fixture.
        /// </summary>
        public SetupStorageFixture Fixture { get; }

        /// <summary>
        /// This is a test.
        /// </summary>
        /// <returns>A task.</returns>
        [Fact]
        public async Task CanGetSingleSpecifiedEntity()
        {
            // Act
            TestEntity getIt = await this.sut.GetSingleAsync("test", "query1");

            // Assert
            Assert.NotNull(getIt);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Correctly implements the dispose pattern.
        /// </summary>
        /// <param name="disposing">true if disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.sut.Dispose();
                    this.sut = null;
                }

                this.disposedValue = true;
            }
        }
    }
}
