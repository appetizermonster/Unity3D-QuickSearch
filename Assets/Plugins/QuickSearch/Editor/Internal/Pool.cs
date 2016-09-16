using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickSearch {

	internal sealed class Pool<T> {
		private readonly Queue<T> queue_ = null;
		private readonly Func<T> allocator_ = null;

		public Pool (Func<T> allocator, int capacity) {
			queue_ = new Queue<T>(capacity);
			allocator_ = allocator;

			for (var i = 0; i < capacity; ++i) {
				var obj = allocator();
				queue_.Enqueue(obj);
			}
		}

		public T Alloc () {
			if (queue_.Count > 0)
				return queue_.Dequeue();
			return allocator_();
		}

		public void Dealloc (T obj) {
			queue_.Enqueue(obj);
		}
	}
}
