// <copyright file="TableAccess.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Generic wrapper for Azure Table access.
    /// </summary>
    /// <typeparam name="T">the type to access from the table.</typeparam>
    public partial class TableAccess<T> : IDisposable, ITableAccess<T>
        where T : class, ITableEntity, new()
    {
        /// <summary> Logging provider. </summary>
        private readonly ILogger<TableAccess<T>> logger;

        /// <summary> the table name. </summary>
        private readonly string tableName;

        /// <summary> To detect redundant calls. </summary>
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAccess{T}"/> class.
        /// </summary>
        /// <param name="table">the table to access.</param>
        /// <param name="logger">An optional logger.</param>
        public TableAccess(TableClient table, ILogger<TableAccess<T>> logger = null)
        {
            ArgumentNullException.ThrowIfNull(table);

            this.logger = logger ?? NullLogger<TableAccess<T>>.Instance;

            this.LogTrace("Table access logging is available.");

            this.TableClient = table;
            this.tableName = table.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAccess{T}"/> class.
        /// </summary>
        /// <param name="connectionString"> azure table connection string. </param>
        /// <param name="tableName"> table name to use for storage. </param>
        /// <param name="logger">A logger implemenation.</param>
        public TableAccess(string connectionString, string tableName, ILogger<TableAccess<T>> logger = null)
            : this(new TableClient(connectionString, tableName), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAccess{T}"/> class from a <see cref="TableServiceClient"/>,
        /// such as one registered with <c>AddAzureClients</c> or created by <c>IAzureClientFactory&lt;TableServiceClient&gt;</c>.
        /// </summary>
        /// <param name="serviceClient">The table service client for the storage account.</param>
        /// <param name="tableName">The name of the table to use for storage.</param>
        /// <param name="logger">An optional logger.</param>
        public TableAccess(TableServiceClient serviceClient, string tableName, ILogger<TableAccess<T>> logger = null)
            : this(GetTableClient(serviceClient, tableName), logger)
        {
        }

        /// <summary> Gets or sets the table to read from. </summary>
        protected internal TableClient TableClient { get; set; }

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
                this.LogNullEntity("insertion");
                return;
            }

            await this.CreateIfNotExists();

            this.LogTrace("Inserting entity");

            Response response = await this.ExecuteAsync(() => this.TableClient.AddEntityAsync(entity));

            this.EnsureSuccess(response.Status);
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
            this.LogKeys(partitionKey, rowKey);

            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
            {
                throw new NotFoundException(partitionKey, rowKey);
            }

            await this.CreateIfNotExists();

            NullableResponse<T> result = await this.ExecuteAsync(() => this.TableClient.GetEntityIfExistsAsync<T>(partitionKey, rowKey));

            if (result.HasValue)
            {
                return result.Value;
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
                this.LogNullEntity("insert or update");
                return;
            }

            await this.CreateIfNotExists();

            Response response = await this.ExecuteAsync(() => this.TableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace));

            this.EnsureSuccess(response.Status);
        }

        /// <summary>
        /// Delete an entity. Deleting an entity that does not exist is a no-op.
        /// </summary>
        /// <param name="entity"> the entity to delete. </param>
        /// <returns>A task.</returns>
        /// <exception cref="StorageException">Thrown if a non success code is returned from deleting the entity from the table.</exception>
        public virtual async Task DeleteAsync(T entity)
        {
            if (entity == null)
            {
                this.LogNullEntity("delete");
                return;
            }

            await this.CreateIfNotExists();

            Response response = await this.ExecuteAsync(() => this.TableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, ETag.All));

            if (response.Status == 404)
            {
                return;
            }

            this.EnsureSuccess(response.Status);
        }

        /// <summary>
        /// Find the first row that has a given property value.
        /// </summary>
        /// <param name="rowKey">the row key.</param>
        /// <param name="propertyName">the property to search on.</param>
        /// <param name="propertyValue">the value of the property.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<T> FindFirstRowWithProperty(string rowKey, string propertyName, string propertyValue)
        {
            if (string.IsNullOrWhiteSpace(rowKey) || string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(propertyValue))
            {
                return null;
            }

            await this.CreateIfNotExists();

            // Property names cannot be parameterized; values are escaped by CreateQueryFilter.
            string filter = $"{TableClient.CreateQueryFilter($"RowKey eq {rowKey}")} and {propertyName} eq {TableClient.CreateQueryFilter($"{propertyValue}")}";

            await foreach (T item in this.TableClient.QueryAsync<T>(filter, maxPerPage: 1))
            {
                return item;
            }

            return null;
        }

        /// <summary>
        /// Find all the items with a given partition key.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>A readonly collection of all the items in the partition.</returns>
        public async Task<IReadOnlyCollection<T>> FindAllByPartitionKey(string partitionKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                return ReadOnlyCollection<T>.Empty;
            }

            return await this.QueryAllAsync(TableClient.CreateQueryFilter($"PartitionKey eq {partitionKey}"));
        }

        /// <summary>
        /// Find all the items with a given row key.
        /// </summary>
        /// <param name="rowKey">The row key.</param>
        /// <returns>A readonly collection of all the items with the given row key.</returns>
        public async Task<IReadOnlyCollection<T>> FindAllByRowKey(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                return ReadOnlyCollection<T>.Empty;
            }

            return await this.QueryAllAsync(TableClient.CreateQueryFilter($"RowKey eq {rowKey}"));
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
                    this.TableClient = null;
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Create the table if it doesn't already exist.
        /// </summary>
        /// <returns>A task. </returns>
        protected virtual async Task CreateIfNotExists()
        {
            this.LogEnsureTable(this.tableName);

            await this.ExecuteAsync(() => this.TableClient.CreateIfNotExistsAsync());
        }

        private static TableClient GetTableClient(TableServiceClient serviceClient, string tableName)
        {
            ArgumentNullException.ThrowIfNull(serviceClient);
            ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

            return serviceClient.GetTableClient(tableName);
        }

        private async Task<IReadOnlyCollection<T>> QueryAllAsync(string filter)
        {
            await this.CreateIfNotExists();

            List<T> results = [];

            await foreach (T item in this.TableClient.QueryAsync<T>(filter))
            {
                results.Add(item);
            }

            return results.AsReadOnly();
        }

        private async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation)
        {
            try
            {
                return await operation();
            }
            catch (RequestFailedException ex)
            {
                this.LogStatusError(ex.Status);
                throw new StorageException($"HttpStatusCode returned is {ex.Status}", ex);
            }
        }

        private void EnsureSuccess(int status)
        {
            if (status >= 200 && status < 300)
            {
                this.LogStatus(status);
                return;
            }

            this.LogStatusError(status);
            throw new StorageException($"HttpStatusCode returned is {status}");
        }

        [LoggerMessage(Level = LogLevel.Trace, Message = "{Message}")]
        private partial void LogTrace(string message);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Null value passed as {Operation} parameter, returning early.")]
        private partial void LogNullEntity(string operation);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Supplied partition key {PartitionKey}, supplied row key {RowKey}.")]
        private partial void LogKeys(string partitionKey, string rowKey);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Ensuring table {TableName} exists.")]
        private partial void LogEnsureTable(string tableName);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Return code is {StatusCode}")]
        private partial void LogStatus(int statusCode);

        [LoggerMessage(Level = LogLevel.Error, Message = "HttpStatusCode returned is {StatusCode}")]
        private partial void LogStatusError(int statusCode);
    }
}
