// <copyright file="TableAccess.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Generic wrapper for Azure Table access.
    /// </summary>
    /// <typeparam name="T">the type to access from the table.</typeparam>
    public class TableAccess<T> : IDisposable, ITableAccess<T>
        where T : class, ITableEntity
    {
        /// <summary> Logging provider. </summary>
        private readonly ILogger<TableAccess<T>> logger;

        /// <summary> To detect redundant calls. </summary>
        private bool disposedValue = false;

        /// <summary> users table. </summary>
        private CloudTable cloudTable;

        /// <summary> table client. </summary>
        private CloudTableClient cloudTableClient;

        /// <summary> storage account. </summary>
        private CloudStorageAccount cloudStorageAccount;

        /// <summary> the table name. </summary>
        private string tableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAccess{T}"/> class.
        /// </summary>
        /// <param name="table">the table to access.</param>
        /// <param name="logger">An optional logger.</param>
        public TableAccess(CloudTable table, ILogger<TableAccess<T>> logger = null)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            this.logger = logger;

            this.logger?.LogTrace("Table access logging is available.");

            this.CloudTable = table;
            this.tableName = table.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAccess{T}"/> class.
        /// </summary>
        /// <param name="connectionString"> azure table connection string. </param>
        /// <param name="tableName"> table name to use for storage. </param>
        /// <param name="logger">A logger implemenation.</param>
        public TableAccess(string connectionString, string tableName, ILogger<TableAccess<T>> logger = null)
        {
            this.logger = logger;

            this.logger?.LogTrace("Table access logging is available.");

            this.cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = this.cloudStorageAccount.CreateCloudTableClient();
            this.CloudTable = this.cloudTableClient.GetTableReference(tableName);
            this.tableName = tableName;
        }

        /// <summary> Gets or sets the table to read from. </summary>
        protected internal CloudTable CloudTable { get => this.cloudTable; set => this.cloudTable = value; }

        /// <summary>
        /// Dispose of managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Add an entity.
        /// </summary>
        /// <param name="entity"> the entity to add. </param>
        /// <returns>A task.</returns>
        /// <exception cref="StorageException">Thrown if a non success code is returned from table storage addition.</exception>
        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
            {
                this.logger?.LogWarning("Null value passed as insertion parameter, returning early.");
                return;
            }

            await this.CreateIfNotExists();

            this.logger?.LogTrace("Inserting entity");

            TableOperation insertOperation = TableOperation.Insert(entity);

            TableResult result = await this.CloudTable.ExecuteAsync(insertOperation);

            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
            {
                this.logger?.LogTrace($"Return code is {result.HttpStatusCode}");
            }
            else
            {
                string message = $"HttpStatusCode returned is {result.HttpStatusCode}";
                this.logger?.LogError(message);
                throw new StorageException(message);
            }
        }

        /// <summary>
        /// Get a single entity based on its partition and row keys.
        /// </summary>
        /// <param name="partitionKey"> the partition key. </param>
        /// <param name="rowKey"> the row key. </param>
        /// <returns>An entity of type T. </returns>
        /// <exception cref="NotFoundException"> Thrown if the entity is not found. </exception>
        public virtual async Task<T> GetSingleAsync(string partitionKey, string rowKey)
        {
            this.logger?.LogTrace($"Supplied partition key {partitionKey}, supplied row key {rowKey}.");

            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
            {
                throw new NotFoundException(partitionKey, rowKey);
            }

            await this.CreateIfNotExists();

            this.logger?.LogTrace($"Retrieving from table with Partition Key {partitionKey} and row key {rowKey}.");

            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);

            TableResult retrievedResult = await this.CloudTable.ExecuteAsync(retrieveOperation);

            this.logger?.LogTrace($"TableResult result is not null - {retrievedResult.Result != null}");

            if (retrievedResult.Result != null)
            {
                return (T)retrievedResult.Result;
            }

            throw new NotFoundException(partitionKey, rowKey);
        }

        /// <summary>
        /// Insert or Replace an entity.
        /// </summary>
        /// <param name="entity"> the entity to insert or replace. </param>
        /// <returns>A task.</returns>
        /// <exception cref="StorageException">Thrown if a non success code is returned from table storage insertion or update.</exception>
        public virtual async Task InsertOrReplaceAsync(T entity)
        {
            if (entity == null)
            {
                this.logger?.LogWarning("Null value passed as insert or update parameter, returning early.");
                return;
            }

            await this.CreateIfNotExists();

            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);

            TableResult result = await this.CloudTable.ExecuteAsync(insertOrReplaceOperation);

            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
            {
                this.logger?.LogTrace($"Return code is {result.HttpStatusCode}");
            }
            else
            {
                string message = $"HttpStatusCode returned is {result.HttpStatusCode}";
                this.logger?.LogError(message);
                throw new StorageException(message);
            }
        }

        /// <summary>
        /// Delete an entity.
        /// </summary>
        /// <param name="entity"> the entity to delete. </param>
        /// <returns>A task.</returns>
        /// <exception cref="StorageException">Thrown if a non success code is returned from deleting the entity from the table.</exception>
        public virtual async Task DeleteAsync(T entity)
        {
            if (entity == null)
            {
                this.logger?.LogWarning("Null value passed as delete parameter, returning early.");
                return;
            }

            await this.CreateIfNotExists();

            TableOperation retrieveOperation = TableOperation.Retrieve<T>(entity.PartitionKey, entity.RowKey);

            TableResult retrieveResult = await this.CloudTable.ExecuteAsync(retrieveOperation);

            T deleteEntity = (T)retrieveResult.Result;

            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                TableResult deleteResult = await this.CloudTable.ExecuteAsync(deleteOperation);

                if (deleteResult.HttpStatusCode >= 200 && deleteResult.HttpStatusCode < 300)
                {
                    this.logger?.LogTrace($"The returned status was {deleteResult.HttpStatusCode}");
                    return;
                }
                else
                {
                    string message = $"Error code {deleteResult.HttpStatusCode}";
                    this.logger?.LogError(message);
                    throw new StorageException(message);
                }
            }
        }

        /// <summary>
        /// Dispose of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">true if called from Dispose, false during finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.CloudTable = null;
                    this.cloudTableClient = null;
                    this.cloudStorageAccount = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Create the table if it doesn't already exist.
        /// </summary>
        /// <returns>A task. </returns>
        protected virtual async Task CreateIfNotExists()
        {
            bool exists = await this.CloudTable.ExistsAsync();

            this.logger?.LogTrace($"Table named {this.tableName} exists - {exists}.");

            if (!exists)
            {
                this.logger?.LogTrace($"Creating table {this.tableName}.");

                await this.CloudTable.CreateAsync();
            }
        }
    }
}
