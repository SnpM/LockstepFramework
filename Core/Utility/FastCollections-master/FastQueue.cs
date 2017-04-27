using System;

namespace FastCollections
{
    public class FastQueue<T> {
        private T[] innerArray;
        private int head;
        private int tail;
        public int Count { get; private set; }
        public int Capacity { get; private set; }

        public FastQueue()
            : this(8) {}

        public FastQueue(int capacity) {
            Capacity = capacity;
            head = 0;
            tail = 0;
            Count = 0;
            innerArray = new T[Capacity];
        }

        public void Add(T item) {
            if (tail == head) {
                SetCapacity(Count + 1);
            }

            innerArray[tail++] = item;
            if (tail == Capacity) {
                tail = 0;
            }

            Count++;
        }

        public T Pop() {
            T ret = innerArray[head];
			innerArray[head] = default(T);
			head++;
            if (head == Capacity) {
                head = 0;
            }
            Count--;
            return ret;
        }
		public void Remove () {
			innerArray[head] = default(T);
			head++;
			if (head == Capacity) {
				head = 0;
			}
			Count--;
		}
        public T Peek () {
            return innerArray[head];
        }
        public T PeekTail () {
            int tailIndex = tail - 1;
            if (tailIndex < 0) tailIndex = this.Capacity - 1;
            return innerArray[tailIndex];
        }

        public void SetCapacity(int min) {
            if (Capacity < min) {
                int prevLength = Capacity;
                Capacity *= 2;
                if (Capacity < min) {
                    Capacity = min;
                }

                var newArray = new T[Capacity];
                if (tail > head) {                                                // If we are not wrapped around...
                    Array.Copy(innerArray, head, newArray, 0, Count);             // ...take from head to head+Count and copy to beginning of new array
                } else if (Count > 0) {                                           // Else if we are wrapped around... (tail == head is ambiguous - could be an empty buffer or a full one)
                    Array.Copy(innerArray, head, newArray, 0, prevLength - head); // ...take head to end and copy to beginning of new array
                    Array.Copy(innerArray, 0, newArray, prevLength - head, tail); // ...take beginning to tail and copy after previously copied elements
                }

                head = 0;
                tail = Count;
                innerArray = newArray;
            }
        }
		public void FullClear () {
			Shortcuts.ClearArray(innerArray);
			FastClear();
		}
        public void FastClear() {
            Count = 0;
            tail = 0;
            head = 0;
        }

        public T[] ToArray() {
            var result = new T[Count];
            if (tail > head) {
                Array.Copy(innerArray, head, result, 0, Count);
            } else if (Count > 0) {
                Array.Copy(innerArray, head, result, 0, Capacity - head);
                Array.Copy(innerArray, 0, result, Capacity - head, tail);
            }
            return result;
        }

    }
}