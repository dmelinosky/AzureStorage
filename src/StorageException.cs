// <copyright file="StorageException.cs" company="Gobie74">
// Copyright (c) Gobie74. All rights reserved.
// </copyright>

namespace Gobie74.AzureStorage
{
    using System;

    /// <summary>
    /// An exception for error conditions in table access.
    /// </summary>
    public class StorageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        public StorageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        /// <param name="innerException">An inner exception.</param>
        public StorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageException"/> class.
        /// </summary>
        public StorageException()
        {
        }
    }
}
