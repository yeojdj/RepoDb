﻿using RepoDb.Enumerations;
using RepoDb.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace RepoDb
{
    /// <summary>
    /// A class used to cache the properties of the entity.
    /// </summary>
    public static class PropertyCache
    {
        private static readonly ConcurrentDictionary<string, IEnumerable<PropertyInfo>> m_cache = new ConcurrentDictionary<string, IEnumerable<PropertyInfo>>();

        /// <summary>
        /// Gets the cached primary key property for the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the target entity.</typeparam>
        /// <param name="command">The target command.</param>
        /// <returns>The cached properties of the entity.</returns>
        public static IEnumerable<PropertyInfo> Get<TEntity>(Command command = Command.None)
            where TEntity : class
        {
            var type = typeof(TEntity);
            var key = $"{type.FullName}.{command.ToString()}";
            var properties = (IEnumerable<PropertyInfo>)null;
            if (m_cache.TryGetValue(key, out properties) == false)
            {
                properties = DataEntityExtension.GetPropertiesFor<TEntity>(command);
                m_cache.TryAdd(key, properties);
            }
            return properties;
        }
    }
}
