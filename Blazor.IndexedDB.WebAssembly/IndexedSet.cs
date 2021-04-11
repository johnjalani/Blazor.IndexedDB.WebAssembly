using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Blazor.IndexedDB.WebAssembly
{
    public interface IIndexedSet : IEnumerable
    {
        /// <summary>
        /// Update internal records. This is called when the IndexedDB reloads its data. It avoids re-creating <see cref="IndexedSet{T}"/>.
        /// </summary>
        /// <param name="records">The new records.</param>
        internal void SetRecords(object records);
    }
    public interface IIndexedSet<T> : IIndexedSet, IEnumerable<T>
    {
        bool IsReadOnly { get; }
        int Count { get; }
        string Name { get; }
        internal void SetRecords(IEnumerable<T> records);
        void Add(T item);
        void Clear();
        bool Contains(T item);
        bool Remove(T item);
    }
    public class IndexedSet<T> : IIndexedSet<T>, IEnumerable<T> where T : new()
    {
        /// <summary>
        /// The internal stored items
        /// </summary>
        private readonly IList<IndexedEntity<T>> internalItems;
        /// <summary>
        /// The type T primary key, only != null if at least once requested by remove
        /// </summary>
        private PropertyInfo primaryKey;

        // ToDo: Remove PK dependency
        public IndexedSet(IEnumerable<T> records, string name, PropertyInfo primaryKey)
        {
            this.Name = name;
            this.primaryKey = primaryKey;
            this.internalItems = new List<IndexedEntity<T>>();
            ((IIndexedSet)this).SetRecords(records);
        }

        public bool IsReadOnly => false;

        public int Count => this.Count();

        public string Name { get; }

        /// <summary>
        /// Update internal records. This is called when the IndexedDB reloads its data. It avoids re-creating <see cref="IndexedSet{T}"/>.
        /// </summary>
        /// <param name="records">The new records.</param>
        void IIndexedSet.SetRecords(object records)
        {
            ((IIndexedSet<T>)this).SetRecords((List<T>)records);
        }
        void IIndexedSet<T>.SetRecords(IEnumerable<T> records)
        {
            this.internalItems.Clear();
            if (records == null)
            {
                return;
            }

            Debug.WriteLine($"{nameof(IndexedEntity)} - Set Records - Add records");

            foreach (var item in records)
            {
                var indexedItem = new IndexedEntity<T>(item)
                {
                    State = EntityState.Unchanged
                };

                this.internalItems.Add(indexedItem);
            }

            Debug.WriteLine($"{nameof(IndexedEntity)} - Set Records - Add records DONE");
        }

        public void Add(T item)
        {
            if (!this.internalItems.Select(x => x.Instance).Contains(item))
            {
                Debug.WriteLine($"{nameof(IndexedEntity)} - Added item of type {typeof(T).Name}");

                this.internalItems.Add(new IndexedEntity<T>(item)
                {
                    State = EntityState.Added
                });
            }
        }

        public void Clear()
        {
            foreach (var item in this)
            {
                this.Remove(item);
            }
        }

        public bool Contains(T item)
        {
            return Enumerable.Contains(this, item);
        }

        public bool Remove(T item)
        {
            var internalItem = this.internalItems.FirstOrDefault(x => x.Instance.Equals(item));

            if (internalItem != null)
            {
                internalItem.State = EntityState.Deleted;

                return true;
            }
            // If reference was lost search for pk, increases the required time
            else
            {
                Debug.WriteLine("Searching for equality with PK");

                var value = this.primaryKey.GetValue(item);

                internalItem = this.internalItems.FirstOrDefault(x => this.primaryKey.GetValue(x.Instance).Equals(value));

                if (internalItem != null)
                {
                    Debug.WriteLine($"Found item with id {value}");

                    internalItem.State = EntityState.Deleted;

                    return true;
                }
            }

            Debug.WriteLine("Could not find internal stored item");
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.internalItems.Select(x => x.Instance).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var enumerator = this.GetEnumerator();

            return enumerator;
        }

        // ToDo: replace change tracker with better alternative
        internal IEnumerable<IndexedEntity> GetChanged()
        {
            foreach (var item in this.internalItems)
            {
                item.DetectChanges();

                if (item.State == EntityState.Unchanged)
                {
                    continue;
                }

                Debug.WriteLine("Item yield");
                yield return item;
            }
        }
    }
}
