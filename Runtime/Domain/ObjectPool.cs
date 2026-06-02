using System;
using System.Collections.Generic;
using SWeetJelly.Pooling.Contract;

namespace SWeetJelly.Pooling.Domain
{
    public sealed class ObjectPool<T> : IPool<T>
        where T : class
    {
        private readonly Stack<T> inactive = new Stack<T>();
        private readonly HashSet<T> inactiveSet = new HashSet<T>();
        private readonly HashSet<T> activeSet = new HashSet<T>();

        private readonly Func<T> create;
        private readonly Action<T> onRent;
        private readonly Action<T> onReturn;
        private readonly Action<T> onDestroy;

        private readonly int maxSize;

        public int ActiveCount => activeSet.Count;
        public int InactiveCount => inactive.Count;
        public int Count => ActiveCount + InactiveCount;

        public ObjectPool(
            Func<T> create,
            Action<T> onRent = null,
            Action<T> onReturn = null,
            Action<T> onDestroy = null,
            int initialSize = 0,
            int maxSize = 0)
        {
            this.create = create ?? throw new ArgumentNullException(nameof(create));
            this.onRent = onRent;
            this.onReturn = onReturn;
            this.onDestroy = onDestroy;

            if (initialSize < 0)
                throw new ArgumentOutOfRangeException(nameof(initialSize), "Initial size cannot be negative.");

            if (maxSize < 0)
                throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size cannot be negative.");

            if (maxSize > 0 && initialSize > maxSize)
                throw new ArgumentException("Initial size cannot be greater than max size.");

            this.maxSize = maxSize;

            Prewarm(initialSize);
        }

        public T Rent()
        {
            if (TryRent(out T item))
                return item;

            throw new InvalidOperationException(
                $"ObjectPool<{typeof(T).Name}> has no available item and cannot grow.");
        }

        public bool TryRent(out T item)
        {
            if (inactive.Count > 0)
            {
                item = inactive.Pop();
                inactiveSet.Remove(item);

                Activate(item);
                return true;
            }

            if (!CanCreateNew())
            {
                item = null;
                return false;
            }

            item = CreateNew();

            Activate(item);
            return true;
        }

        public void Return(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!activeSet.Remove(item))
            {
                throw new InvalidOperationException(
                    $"ObjectPool<{typeof(T).Name}> cannot return an item that is not active in this pool.");
            }

            try
            {
                onReturn?.Invoke(item);
            }
            catch
            {
                activeSet.Add(item);
                throw;
            }

            inactive.Push(item);
            inactiveSet.Add(item);
        }

        public bool Contains(T item)
        {
            if (item == null)
                return false;

            return activeSet.Contains(item) || inactiveSet.Contains(item);
        }

        public bool IsActive(T item)
        {
            if (item == null)
                return false;

            return activeSet.Contains(item);
        }

        public void Clear()
        {
            var items = new List<T>(Count);

            while (inactive.Count > 0)
            {
                items.Add(inactive.Pop());
            }

            foreach (T item in activeSet)
            {
                items.Add(item);
            }

            inactiveSet.Clear();
            activeSet.Clear();

            if (onDestroy == null)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                onDestroy(items[i]);
            }
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T item = CreateNew();

                inactive.Push(item);
                inactiveSet.Add(item);
            }
        }

        private bool CanCreateNew()
        {
            return maxSize <= 0 || Count < maxSize;
        }

        private T CreateNew()
        {
            T item = create();

            if (item == null)
            {
                throw new InvalidOperationException(
                    $"ObjectPool<{typeof(T).Name}> create function returned null.");
            }

            return item;
        }

        private void Activate(T item)
        {
            if (!activeSet.Add(item))
            {
                throw new InvalidOperationException(
                    $"ObjectPool<{typeof(T).Name}> item is already active.");
            }

            try
            {
                onRent?.Invoke(item);
            }
            catch
            {
                activeSet.Remove(item);

                inactive.Push(item);
                inactiveSet.Add(item);

                throw;
            }
        }
    }
}