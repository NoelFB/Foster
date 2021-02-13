/*
    MIT License

    Copyright (c) 2019 Bar Arnon

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

    ==============================================================================

    CoreFX (https://github.com/dotnet/corefx)
    The MIT License (MIT)
    Copyright (c) .NET Foundation and Contributors
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Foster.Framework
{
    /// <summary>
    /// Represents a thread-safe hash-based unique collection.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection.</typeparam>
    /// <remarks>
    /// All public members of <see cref="ConcurrentHashSet{T}"/> are thread-safe and may be used
    /// concurrently from multiple threads.
    /// </remarks>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class ConcurrentHashSet<T> : IReadOnlyCollection<T>, ICollection<T>
    {
        private const int DEFAULT_CAPACITY = 31;
        private const int MAX_LOCK_NUMBER = 1024;

        private readonly IEqualityComparer<T> comparer;
        private readonly bool growLockArray;

        private int budget;
        private volatile Tables tables;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ConcurrentHashSet{T}"/>
        /// class that is empty, has the default concurrency level, has the default initial capacity, and
        /// uses the default comparer for the item type.
        /// </summary>
        public ConcurrentHashSet() : this(DefaultConcurrencyLevel, DEFAULT_CAPACITY, true, null) { }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ConcurrentHashSet{T}"/>
        /// class that is empty, has the specified concurrency level and capacity, and uses the default
        /// comparer for the item type.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the
        /// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see
        /// cref="ConcurrentHashSet{T}"/>
        /// can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="concurrencyLevel"/> is
        /// less than 1.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"> <paramref name="capacity"/> is less than
        /// 0.</exception>
        public ConcurrentHashSet(int concurrencyLevel, int capacity) : this(concurrencyLevel, capacity, false, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/>
        /// class that is empty, has the specified concurrency level and capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>
        /// implementation to use when comparing items.</param>
        public ConcurrentHashSet(IEqualityComparer<T>? comparer) : this(
            DefaultConcurrencyLevel,
            DEFAULT_CAPACITY,
            true,
            comparer) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/>
        /// class that contains elements copied from the specified <see
        /// cref="T:System.Collections.IEnumerable"/>, has the default concurrency level, has the default
        /// initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="collection">The <see
        /// cref="T:System.Collections.IEnumerable{T}"/> whose elements are copied to
        /// the new
        /// <see cref="ConcurrentHashSet{T}"/>.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>
        /// implementation to use when comparing items.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="collection"/> is a null reference
        /// (Nothing in Visual Basic).
        /// </exception>
        public ConcurrentHashSet(IEnumerable<T>? collection, IEqualityComparer<T>? comparer = null) : this(comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            InitializeFromCollection(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/>
        /// class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable"/>,
        /// has the specified concurrency level, has the specified initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the
        /// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{T}"/> whose elements are copied to the new
        /// <see cref="ConcurrentHashSet{T}"/>.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/> implementation to use
        /// when comparing items.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="collection"/> is a null reference.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="concurrencyLevel"/> is less than 1.
        /// </exception>
        public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T>? collection, IEqualityComparer<T>? comparer) :
            this(concurrencyLevel, DEFAULT_CAPACITY, false, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            InitializeFromCollection(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/>
        /// class that is empty, has the specified concurrency level, has the specified initial capacity, and
        /// uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the
        /// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see
        /// cref="ConcurrentHashSet{T}"/>
        /// can contain.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>
        /// implementation to use when comparing items.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="concurrencyLevel"/> is less than 1. -or-
        /// <paramref name="capacity"/> is less than 0.
        /// </exception>
        public ConcurrentHashSet(int concurrencyLevel, int capacity, IEqualityComparer<T>? comparer) : this(
            concurrencyLevel,
            capacity,
            false,
            comparer) { }

        private ConcurrentHashSet(
            int concurrencyLevel,
            int capacity,
            bool growLockArray,
            IEqualityComparer<T>? comparer)
        {
            if (concurrencyLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(concurrencyLevel));
            }

            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            // The capacity should be at least as large as the concurrency level. Otherwise, we would have locks that don't guard
            // any buckets.
            if (capacity < concurrencyLevel)
            {
                capacity = concurrencyLevel;
            }

            var locks = new object[concurrencyLevel];
            for (var i = 0; i < locks.Length; i++)
            {
                locks[i] = new object();
            }

            var countPerLock = new int[locks.Length];
            var buckets = new Node[capacity];
            tables = new Tables(buckets, locks, countPerLock);

            this.growLockArray = growLockArray;
            budget = buckets.Length / locks.Length;
            this.comparer = comparer ?? EqualityComparer<T>.Default;
        }

        private static int DefaultConcurrencyLevel => Environment.ProcessorCount;

        /// <summary>
        /// Gets a value that indicates whether the <see cref="ConcurrentHashSet{T}"/> is empty.
        /// </summary>
        /// <value>true if the <see cref="ConcurrentHashSet{T}"/> is empty; otherwise,
        /// false.</value>
        public bool IsEmpty
        {
            get
            {
                var acquiredLocks = 0;
                try
                {
                    AcquireAllLocks(ref acquiredLocks);

                    for (var i = 0; i < tables.CountPerLock.Length; i++)
                    {
                        if (tables.CountPerLock[i] != 0)
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    ReleaseLocks(0, acquiredLocks);
                }

                return true;
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="ConcurrentHashSet{T}"/>.
        /// </summary>
        public void Clear()
        {
            var locksAcquired = 0;
            try
            {
                AcquireAllLocks(ref locksAcquired);

                var newTables = new Tables(
                    new Node[DEFAULT_CAPACITY],
                    tables.Locks,
                    new int[tables.CountPerLock.Length]);

                tables = newTables;
                budget = Math.Max(1, newTables.Buckets!.Length / newTables.Locks!.Length);
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="ConcurrentHashSet{T}"/> contains the specified
        /// item.
        /// </summary>
        /// <param name="item">The item to locate in the <see cref="ConcurrentHashSet{T}"/>.</param>
        /// <returns>true if the <see cref="ConcurrentHashSet{T}"/> contains the item; otherwise, false.</returns>
        public bool Contains(T item)
        {
            var hashcode = item != null ? comparer.GetHashCode(item) : 0;

            // We must capture the _buckets field in a local variable. It is set to a new table on each table resize.
            var transientTables = tables;

            var bucketIndex = GetBucket(hashcode, transientTables.Buckets.Length);

            // We can get away w/out a lock here.
            // The Volatile.Read ensures that the load of the fields of 'n' doesn't move before the load from buckets[i].
            var current = Volatile.Read(ref transientTables.Buckets[bucketIndex]);

            while (current != null)
            {
                if ((hashcode == current.Hashcode) && comparer.Equals(current.Item, item))
                {
                    return true;
                }

                current = current.Next;
            }

            return false;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            var locksAcquired = 0;
            try
            {
                AcquireAllLocks(ref locksAcquired);

                var count = 0;

                for (var i = 0; (i < tables.Locks.Length) && (count >= 0); i++)
                {
                    count += tables.CountPerLock[i];
                }

                // "count" itself or "count + arrayIndex" can overflow
                if (((array.Length - count) < arrayIndex) || (count < 0))
                {
                    throw new ArgumentException(
                        "The index is equal to or greater than the length of the array, or the number of elements in the set is greater than the available space from index to the end of the destination array.");
                }

                CopyToItems(array, arrayIndex);
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            return TryRemove(item);
        }

        /// <summary>
        /// Gets the number of items contained in the <see
        /// cref="ConcurrentHashSet{T}"/>.
        /// </summary>
        /// <value>The number of items contained in the <see
        /// cref="ConcurrentHashSet{T}"/>.</value>
        /// <remarks>Count has snapshot semantics and represents the number of items in the <see
        /// cref="ConcurrentHashSet{T}"/>
        /// at the moment when Count was accessed.</remarks>
        public int Count
        {
            get
            {
                var count = 0;
                var acquiredLocks = 0;
                try
                {
                    AcquireAllLocks(ref acquiredLocks);

                    for (var i = 0; i < tables.CountPerLock.Length; i++)
                    {
                        count += tables.CountPerLock[i];
                    }
                }
                finally
                {
                    ReleaseLocks(0, acquiredLocks);
                }

                return count;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator
        {
            private readonly Node[] mBuckets;

            private int mIndex;
            private Node? mCurrentNode;

            public T Current => mCurrentNode!.Item;

            internal Enumerator(ConcurrentHashSet<T> hashSet)
            {
                mBuckets = hashSet.tables.Buckets;

                mIndex = 0;
                mCurrentNode = null;
            }

            public bool MoveNext()
            {
                for (; mIndex < mBuckets.Length; ++mIndex)
                {
                    mCurrentNode = mCurrentNode == null ? Volatile.Read(ref mBuckets[mIndex]) : mCurrentNode.Next;

                    while (mCurrentNode != null)
                    {
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                mIndex = 0;
                mCurrentNode = null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the <see
        /// cref="ConcurrentHashSet{T}"/>.</summary>
        /// <returns>An enumerator for the <see cref="ConcurrentHashSet{T}"/>.</returns>
        /// <remarks>
        /// The enumerator returned from the collection is safe to use concurrently with
        /// reads and writes to the collection, however it does not represent a moment-in-time snapshot
        /// of the collection.  The contents exposed through the enumerator may contain modifications
        /// made to the collection after <see cref="GetEnumerator"/> was called.
        /// </remarks>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            var buckets = tables.Buckets;

            for (var i = 0; i < buckets.Length; i++)
            {
                // The Volatile.Read ensures that the load of the fields of 'current' doesn't move before the load from buckets[i].
                var current = Volatile.Read(ref buckets[i]);

                while (current != null)
                {
                    yield return current.Item;

                    current = current.Next;
                }
            }
        }

        /// <summary>
        /// Adds the specified item to the <see cref="ConcurrentHashSet{T}"/>.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>true if the items was added to the <see cref="ConcurrentHashSet{T}"/>
        /// successfully; false if it already exists.</returns>
        /// <exception cref="T:System.OverflowException">The <see cref="ConcurrentHashSet{T}"/>
        /// contains too many items.</exception>
        public bool Add(T item)
        {
            return AddInternal(item, item != null ? comparer.GetHashCode(item) : 0, true);
        }

        /// <summary>
        /// Attempts to remove the item from the <see cref="ConcurrentHashSet{T}"/>.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>true if an item was removed successfully; otherwise, false.</returns>
        public bool TryRemove(T item)
        {
            var hashcode = item != null ? comparer.GetHashCode(item) : 0;
            while (true)
            {
                var transientTables = tables;

                GetBucketAndLockIndex(
                    hashcode,
                    out var bucketIndex,
                    out var lockIndex,
                    transientTables.Buckets.Length,
                    transientTables.Locks.Length);

                lock (transientTables.Locks[lockIndex])
                {
                    // If the table just got resized, we may not be holding the right lock, and must retry.
                    // This should be a rare occurrence.
                    if (transientTables != tables)
                    {
                        continue;
                    }

                    Node? previous = null;
                    for (var current = transientTables.Buckets[bucketIndex]; current != null; current = current.Next)
                    {
                        Debug.Assert(
                            ((previous == null) && (current == transientTables.Buckets[bucketIndex])) ||
                            (previous?.Next == current));

                        if ((hashcode == current.Hashcode) && comparer.Equals(current.Item, item))
                        {
                            if (previous == null)
                            {
                                Volatile.Write(ref transientTables.Buckets[bucketIndex]!, current.Next);
                            }
                            else
                            {
                                previous.Next = current.Next;
                            }

                            transientTables.CountPerLock[lockIndex]--;
                            return true;
                        }

                        previous = current;
                    }
                }

                return false;
            }
        }

        private void InitializeFromCollection(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                AddInternal(item, item != null ? comparer.GetHashCode(item) : 0, false);
            }

            if (budget == 0)
            {
                budget = tables.Buckets.Length / tables.Locks.Length;
            }
        }

        private bool AddInternal(T item, int hashcode, bool acquireLock)
        {
            while (true)
            {
                var transientTables = tables;

                GetBucketAndLockIndex(
                    hashcode,
                    out var bucketIndex,
                    out var lockIndex,
                    transientTables.Buckets.Length,
                    transientTables.Locks.Length);

                var resizeDesired = false;
                var lockTaken = false;
                try
                {
                    if (acquireLock)
                    {
                        global::System.Threading.Monitor.Enter(transientTables.Locks[lockIndex], ref lockTaken);
                    }

                    // If the table just got resized, we may not be holding the right lock, and must retry.
                    // This should be a rare occurrence.
                    if (transientTables != this.tables)
                    {
                        continue;
                    }

                    // Try to find this item in the bucket
                    Node? previous = null;
                    for (var current = transientTables.Buckets[bucketIndex]; current != null; current = current.Next)
                    {
                        Debug.Assert(
                            ((previous == null) && (current == transientTables.Buckets[bucketIndex])) ||
                            (previous?.Next == current));

                        if ((hashcode == current.Hashcode) && comparer.Equals(current.Item, item))
                        {
                            return false;
                        }

                        previous = current;
                    }

                    // The item was not found in the bucket. Insert the new item.
                    Volatile.Write(
                        ref transientTables.Buckets[bucketIndex],
                        new Node(item, hashcode, transientTables.Buckets[bucketIndex]));

                    checked
                    {
                        transientTables.CountPerLock[lockIndex]++;
                    }

                    //
                    // If the number of elements guarded by this lock has exceeded the budget, resize the bucket table.
                    // It is also possible that GrowTable will increase the budget but won't resize the bucket table.
                    // That happens if the bucket table is found to be poorly utilized due to a bad hash function.
                    //
                    if (transientTables.CountPerLock[lockIndex] > budget)
                    {
                        resizeDesired = true;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        global::System.Threading.Monitor.Exit(transientTables.Locks[lockIndex]);
                    }
                }

                //
                // The fact that we got here means that we just performed an insertion. If necessary, we will grow the table.
                //
                // Concurrency notes:
                // - Notice that we are not holding any locks at when calling GrowTable. This is necessary to prevent deadlocks.
                // - As a result, it is possible that GrowTable will be called unnecessarily. But, GrowTable will obtain lock 0
                //   and then verify that the table we passed to it as the argument is still the current table.
                //
                if (resizeDesired)
                {
                    GrowTable(transientTables);
                }

                return true;
            }
        }

        private static int GetBucket(int hashcode, int bucketCount)
        {
            var bucketIndex = (hashcode & 0x7fffffff) % bucketCount;
            Debug.Assert((bucketIndex >= 0) && (bucketIndex < bucketCount));
            return bucketIndex;
        }

        private static void GetBucketAndLockIndex(
            int hashcode,
            out int bucketIndex,
            out int lockIndex,
            int bucketCount,
            int lockCount)
        {
            bucketIndex = (hashcode & 0x7fffffff) % bucketCount;
            lockIndex = bucketIndex % lockCount;

            Debug.Assert((bucketIndex >= 0) && (bucketIndex < bucketCount));
            Debug.Assert((lockIndex >= 0) && (lockIndex < lockCount));
        }

        private void GrowTable(Tables transientTables)
        {
            const int maxArrayLength = 0X7FEFFFFF;
            var locksAcquired = 0;
            try
            {
                // The thread that first obtains _locks[0] will be the one doing the resize operation
                AcquireLocks(0, 1, ref locksAcquired);

                // Make sure nobody resized the table while we were waiting for lock 0:
                if (transientTables != tables)
                {
                    // We assume that since the table reference is different, it was already resized (or the budget
                    // was adjusted). If we ever decide to do table shrinking, or replace the table for other reasons,
                    // we will have to revisit this logic.
                    return;
                }

                // Compute the (approx.) total size. Use an Int64 accumulation variable to avoid an overflow.
                long approxCount = 0;
                for (var i = 0; i < transientTables.CountPerLock.Length; i++)
                {
                    approxCount += transientTables.CountPerLock[i];
                }

                //
                // If the bucket array is too empty, double the budget instead of resizing the table
                //
                if (approxCount < (transientTables.Buckets.Length / 4))
                {
                    budget = 2 * budget;
                    if (budget < 0)
                    {
                        budget = int.MaxValue;
                    }

                    return;
                }

                // Compute the new table size. We find the smallest integer larger than twice the previous table size, and not divisible by
                // 2,3,5 or 7. We can consider a different table-sizing policy in the future.
                var newLength = 0;
                var maximizeTableSize = false;
                try
                {
                    checked
                    {
                        // Double the size of the buckets table and add one, so that we have an odd integer.
                        newLength = (transientTables.Buckets.Length * 2) + 1;

                        // Now, we only need to check odd integers, and find the first that is not divisible
                        // by 3, 5 or 7.
                        while (((newLength % 3) == 0) || ((newLength % 5) == 0) || ((newLength % 7) == 0))
                        {
                            newLength += 2;
                        }

                        Debug.Assert((newLength % 2) != 0);

                        if (newLength > maxArrayLength)
                        {
                            maximizeTableSize = true;
                        }
                    }
                }
                catch (OverflowException)
                {
                    maximizeTableSize = true;
                }

                if (maximizeTableSize)
                {
                    newLength = maxArrayLength;

                    // We want to make sure that GrowTable will not be called again, since table is at the maximum size.
                    // To achieve that, we set the budget to int.MaxValue.
                    //
                    // (There is one special case that would allow GrowTable() to be called in the future:
                    // calling Clear() on the ConcurrentHashSet will shrink the table and lower the budget.)
                    budget = int.MaxValue;
                }

                // Now acquire all other locks for the table
                AcquireLocks(1, transientTables.Locks.Length, ref locksAcquired);

                var newLocks = transientTables.Locks;

                // Add more locks
                if (growLockArray && (transientTables.Locks.Length < MAX_LOCK_NUMBER))
                {
                    newLocks = new object[transientTables.Locks.Length * 2];
                    Array.Copy(transientTables.Locks, 0, newLocks, 0, transientTables.Locks.Length);
                    for (var i = transientTables.Locks.Length; i < newLocks.Length; i++)
                    {
                        newLocks[i] = new object();
                    }
                }

                var newBuckets = new Node[newLength];
                var newCountPerLock = new int[newLocks.Length];

                // Copy all data into a new table, creating new nodes for all elements
                for (var i = 0; i < transientTables.Buckets.Length; i++)
                {
                    var current = transientTables.Buckets[i];
                    while (current != null)
                    {
                        var next = current.Next;
                        GetBucketAndLockIndex(
                            current.Hashcode,
                            out var newBucketNo,
                            out var newLockNo,
                            newBuckets.Length,
                            newLocks.Length);

                        newBuckets[newBucketNo] = new Node(current.Item, current.Hashcode, newBuckets[newBucketNo]);

                        checked
                        {
                            newCountPerLock[newLockNo]++;
                        }

                        current = next;
                    }
                }

                // Adjust the budget
                budget = Math.Max(1, newBuckets.Length / newLocks.Length);

                // Replace tables with the new versions
                this.tables = new Tables(newBuckets, newLocks, newCountPerLock);
            }
            finally
            {
                // Release all locks that we took earlier
                ReleaseLocks(0, locksAcquired);
            }
        }

        private void AcquireAllLocks(ref int locksAcquired)
        {
            // First, acquire lock 0
            AcquireLocks(0, 1, ref locksAcquired);

            // Now that we have lock 0, the _locks array will not change (i.e., grow),
            // and so we can safely read _locks.Length.
            AcquireLocks(1, tables.Locks.Length, ref locksAcquired);
            Debug.Assert(locksAcquired == tables.Locks.Length);
        }

        private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
        {
            Debug.Assert(fromInclusive <= toExclusive);
            var locks = tables.Locks;

            for (var i = fromInclusive; i < toExclusive; i++)
            {
                var lockTaken = false;
                try
                {
                    global::System.Threading.Monitor.Enter(locks[i], ref lockTaken);
                }
                finally
                {
                    if (lockTaken)
                    {
                        locksAcquired++;
                    }
                }
            }
        }

        private void ReleaseLocks(int fromInclusive, int toExclusive)
        {
            Debug.Assert(fromInclusive <= toExclusive);

            for (var i = fromInclusive; i < toExclusive; i++)
            {
                global::System.Threading.Monitor.Exit(tables.Locks[i]);
            }
        }

        private void CopyToItems(IList<T> array, int index)
        {
            var buckets = tables.Buckets;
            for (var i = 0; i < buckets.Length; i++)
            {
                for (var current = buckets[i]; current != null; current = current.Next)
                {
                    array[index] = current.Item;
                    index++; // this should never flow, CopyToItems is only called when there's no overflow risk
                }
            }
        }

        private class Tables
        {
            public readonly Node[] Buckets;
            public readonly object[] Locks;

            public volatile int[] CountPerLock;

            public Tables(Node[] buckets, object[] locks, int[] countPerLock)
            {
                Buckets = buckets;
                Locks = locks;
                CountPerLock = countPerLock;
            }
        }

        private class Node
        {
            public readonly int Hashcode;
            public readonly T Item;

            public volatile Node? Next;

            public Node(T item, int hashcode, Node next)
            {
                Item = item;
                Hashcode = hashcode;
                Next = next;
            }
        }
    }
}
