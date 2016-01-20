using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// A Priority Queue implementation.
/// Items can be added to the queue with ordering based on priorities. The lower the priority, the closer the item is to the front of the queue.
/// The priorityQueue is basically accessed by [CollectionMethodName]WithPriority methods or the standard IList methods.
/// 
/// Copyright (c) 2016 Lam Hoang Pham
/// Distributed under the MIT License.
/// (See accompanying file LICENSE or copy at
/// http://opensource.org/licenses/MIT)
/// </summary>
namespace Lockstep
{
    public class PriorityQueue<T> : IList<T>
    {
        /// <summary>
        /// The nodes are contained in a simple List collection.
        /// We can replace this with a fast list implementation during optimization if needed but the List provides a clarity of the purpose of the Queue for now.
        /// </summary>
        protected List<PriorityQueueNode<T>> _nodes;
        
        public PriorityQueue()
        {
            _nodes = new List<PriorityQueueNode<T>>();
        }

        public PriorityQueue(int capacity)
        {
            _nodes = new List<PriorityQueueNode<T>>(capacity);
        }

        public PriorityQueue(IEnumerable<PriorityQueueNode<T>> collection)
        {
            _nodes = new List<PriorityQueueNode<T>>();
            foreach(var item in collection)
            {
                AddWithPriority(item.Item, item.Priority);
            }
        }

        /// <summary>
        /// Returns the node list in our queue in case we need access to all nodes.
        /// Since this list isn't copied. Be careful of modifying the priorities or you break the priority sorting
        /// </summary>
        public List<PriorityQueueNode<T>> Nodes
        {
            get { return _nodes; }
            set { _nodes = value; }
        }

        /// <summary>
        /// Adds the item based on it's priority within the queue
        /// </summary>
        /// <example>
        /// Adding to a queue in different ways:
        /// <code>
        ///     aQueue.Add("hey!");     //  Adds an item with a default priority of 0
        ///     aQueue.AddWithPriority("world", 100);   //  Adds an item specifing the priorty is 100
        /// </code>
        /// 
        /// You can add items with duplicate priorities or items in in ordering (assuming string elements):
        /// <code>
        ///     aQueue.AddWithPriority("item203", 203);   //  Adds an item specifing the priorty is 100
        ///     aQueue.AddWithPriority("test1", 100);   //  Adds an item specifing the priorty is 100
        ///     aQueue.AddWithPriority("world2", 100);   //  Adds an item specifing the priorty is 100
        ///     aQueue.AddWithPriority("test2", 100);   //  Adds an item specifing the priorty is 100
        ///     aQueue.AddWithPriority("hey", -100);   //  Adds an item specifing the priorty is 100
        ///     aQueue.AddWithPriority("man", -100);   //  Adds an item specifing the priorty is 100
        ///     aQueue.AddWithPriority("between", 150);   //  Adds an item specifing the priorty is 100
        /// </code>
        ///     You will have a queue: ["hey", "man", "test1", "world2", "test2", "between", "item203"]
        /// </example>
        /// <param name="item"></param>
        /// <param name="priority">
        /// The priority determines the placement of the value in the queue. A larger priority places it nearer to the end of the queue.
        /// You can duplicate priorities, the ordering of items is based when it's inserted. The last item inserted is last in the priority section of the queue
        /// </param>
        /// <seealso cref="Add(T)"/>
        public void AddWithPriority(T item, int priority)
        {
            int index = IndexOfWithPriority(item, priority);
            _nodes.Insert(index < 0 ? ~index : index, new PriorityQueueNode<T>(item, priority));
        }

        /// <summary>
        /// Returns the index of the node that has the item with a priority
        /// Otherwise it returns a negative number should the value and the priority not match.
        /// This operation is much faster than IndexOf since the priority hint reduces the lookup to o(logn + k) where k is the number of duplicate priorities
        /// although it assumes that you know the priority of the matching item.
        /// </summary>
        /// <param name="item">The item that to match</param>
        /// <param name="priority">The priority to match</param>
        /// <returns>An index in our array that has an item with a matching value and priority. If negative then the item does not exist.
        /// </returns>
        public int IndexOfWithPriority(T item, int priority)
        {

            var node = new PriorityQueueNode<T>(item, priority);
            var comparer = new PriorityQueueNode<T>.PriorityComparer();

            //  This searches for the closest existing priority
            int index = _nodes.BinarySearch(node, comparer);
            if (index >= 0)
            {
                //  Since we allow duplicate priority values, we must search each duplicate
                //  after the binary search.
                //  Searching order is important since the index we return gives us a suitable
                //  spot for inserting new nodes.

                //  First check if the index node already matches our item
                if (node.Equals(_nodes[index]))
                {
                    return index;
                }

                //  Split the index and find if the item is before our pivot
                int i;
                for (i = index - 1; i >= 0; i--)
                {
                    int c = comparer.Compare(node, _nodes[i]);
                    if (node.Equals(_nodes[i]))
                        return i;
                    else if (c > 0)
                        break;
                }

                //  Since it's not before the pivot, search items after pivot
                for (i = index + 1; i < _nodes.Count; i++)
                {
                    int c = comparer.Compare(node, _nodes[i]);
                    if (node.Equals(_nodes[i]))
                        return i;
                    else if (c < 0)
                        break;
                }
                //  bitwise complement to negate the index position since the item doesn't exist and allows us to use this index for a suitable insertion point
                return ~i;

            }
            return index;
        }

        /// <summary>
        /// Returns true if the item strictly matches a value and a priority
        /// This operation is much faster than Contains since the priority hint reduces the lookup to o(logn + k) where k is the number of duplicate priorities
        /// although it assumes that you know the priority of the matching item.
        /// </summary>
        /// <param name="item">The item that to match</param>
        /// <param name="priority">The priority to match</param>
        /// <returns>if our array that has an item with a matching value and priority</returns>
        public bool ContainsWithPriority(T item, int priority)
        {
            return IndexOfWithPriority(item, priority) >= 0;
        }

        /// <summary>
        /// Remove if there exists an item with a priority.
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <param name="priority">priority to remove</param>
        /// <returns>if the removal was successful</returns>
        public bool RemoveWithPriority(T item, int priority)
        {
            int index = IndexOfWithPriority(item, priority);
            if (index >= 0)
                RemoveAt(index);
            return index >= 0;
        }

        #region IList implementation

        /// <summary>
        /// index operator that returns an item in the queue
        /// </summary>
        /// <param name="index">index of item in queue</param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return _nodes[index].Item; }
            set { _nodes[index].Item = value; }
        }

        /// <summary>
        /// The amount of items in the queue
        /// </summary>
        public int Count
        {
            get { return _nodes.Count; }
        }
        
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Adds an item with the default priority.
        /// You are allowed to have duplicate priorities, in which case the item is at the end of the queue based on insertion time
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            AddWithPriority(item, PriorityQueueNode<T>.DefaultPriority);
        }

        /// <summary>
        /// Clears all the items in the queue
        /// </summary>
        public void Clear()
        {
            _nodes.Clear();
        }

        /// <summary>
        /// Checks if an item is in our queue.
        /// Use this instead of ContainsWithPriority if you are unsure of the priority of the item.
        /// It is an O(n) operation since we cannot be sure where the item is in the list.
        /// </summary>
        /// <param name="item">the item to match</param>
        /// <returns>if an item matches</returns>
        public bool Contains(T item)
        {
            return _nodes.Contains(new PriorityQueueNode<T>(item));
        }

        /// <summary>
        /// Copy to another array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException();
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException();
            if (_nodes.Count - arrayIndex > array.Length)
                throw new ArgumentException("The number of elements in the source PriorityQueue<T> is greater than the available space from arrayIndex to the end of the destination array.");

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _nodes[arrayIndex + i].Item;
            }
        }

        /// <summary>
        /// Allows the use of foreach directly on the queue. Returns items based on it's priority, from the lowest number to the highest.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _nodes.Count; ++i)
                yield return _nodes[i].Item;
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /// <summary>
        /// Get index of item in our queue.
        /// Use this instead of IndexOfWithPriority if you are unsure of the priority of the item.
        /// It is an O(n) operation since we cannot be sure where the item is in the list.
        /// </summary>
        /// <param name="item">the item to match</param>
        /// <returns>index of the item in the queue, negative if missing</returns>
        public int IndexOf(T item)
        {
            return _nodes.IndexOf(new PriorityQueueNode<T>(item));
        }

        /// <summary>
        /// Since we cannot insert at any index of our queue. We insert with a default priority. This is the same as the Add method
        /// </summary>
        /// <param name="index">index is unused</param>
        /// <param name="item">item to insert</param>
        public void Insert(int index, T item)
        {
            AddWithPriority(item, PriorityQueueNode<T>.DefaultPriority);
        }

        /// <summary>
        /// Remove the item
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            return _nodes.Remove(new PriorityQueueNode<T>(item));
        }

        /// <summary>
        /// Remove the item at the index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            _nodes.RemoveAt(index);
        }

        #endregion
    }

    /// <summary>
    /// A priority queue node is an element in our priority queue container that stores the item we want to insert along with it's priority order.
    /// The equality method is redone so that comparisons are done on the item rather than the node.
    /// </summary>
    /// <typeparam name="T">the type being stored in our queue</typeparam>
    public class PriorityQueueNode<T> : IEquatable<T>
    {
        public const int DefaultPriority = 0;
        private int _priority;
        private T _item;

        public PriorityQueueNode(T item)
        {
            _priority = DefaultPriority;
            _item = item;
        }

        public PriorityQueueNode(T item, int priority)
        {
            _item = item;
            _priority = priority;
        }

        /// <summary>
        /// Priority determines the order of nodes in our queue
        /// </summary>
        public int Priority
        {
            get
            {
                return _priority;
            }

            set
            {
                _priority = value;
            }
        }

        /// <summary>
        /// The item value
        /// </summary>
        public T Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
            }
        }

        public bool Equals(T other)
        {
            return _item.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return _item.Equals(((PriorityQueueNode<T>)obj).Item);
        }

        public override int GetHashCode()
        {
            return _item.GetHashCode();
        }

        public override string ToString()
        {
            return "[" + Priority + "]=>" + Item.ToString();
        }

        /// <summary>
        /// Comparer based on the node priority
        /// </summary>
        public class PriorityComparer : IComparer<PriorityQueueNode<T>>
        {
            public int Compare(PriorityQueueNode<T> x, PriorityQueueNode<T> y)
            {
                return x.Priority - y.Priority;
            }
        }
    }

}
