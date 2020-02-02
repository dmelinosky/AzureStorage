// <copyright file="ITableAccess.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage
{
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Interface to interact with Azure Table storage.
    /// </summary>
    /// <typeparam name="T">The type stored in the table.</typeparam>
    public interface ITableAccess<T>
        where T : class, ITableEntity
    {
        /// <summary>
        /// Add an entity.
        /// </summary>
        /// <param name="entity">The Entity to add.</param>
        /// <returns>A table result.</returns>
        Task AddAsync(T entity);

        /// <summary>
        /// Delete an entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A table result.</returns>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Get a single entity given the partition and row keys.
        /// </summary>
        /// <param name="partitionKey">The Partition key.</param>
        /// <param name="rowKey">The Row key.</param>
        /// <returns>A table entity.</returns>
        Task<T> GetSingleAsync(string partitionKey, string rowKey);

        /// <summary>
        /// Insert or replace an entity.
        /// </summary>
        /// <param name="entity">The entity to insert or replace.</param>
        /// <returns>A table result.</returns>
        Task InsertOrReplaceAsync(T entity);
    }
}