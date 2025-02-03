using System;
using System.Diagnostics;

namespace DateKit;

/// <summary>
/// Represents a binary min-heap.
/// </summary>
internal struct Heap<TElement, TKey>
{
	[DebuggerDisplay("Key = {Key}")]
	private readonly struct Node
	{
		public readonly TElement Element;
		public readonly TKey Key;

		public Node(TElement element, TKey key)
		{
			this.Element = element;
			this.Key = key;
		}
	}

	private readonly Comparison<TKey> _comparison;
	private readonly Node[] _nodes;
	private Int32 _count;

	public Heap(Comparison<TKey> comparison, Int32 capacity)
	{
		Debug.Assert(comparison != null);
		Debug.Assert(capacity >= 0);

		_comparison = comparison;
		_nodes = new Node[capacity];
		_count = 0;
	}

	public readonly Int32 Count => _count;

	public void Append(TElement element, TKey key)
	{
		_nodes[_count++] = new Node(element, key);
	}

	public void Heapify()
	{
		Int32 count = _count;

		if (count > 1)
		{
			Node[] nodes = _nodes;
			for (Int32 index = GetParentIndex(count - 1); index >= 0; --index)
				this.SiftDown(index, nodes[index]);
		}
	}

	public TElement GetRoot()
	{
		Debug.Assert(_count > 0);
		return _nodes[0].Element;
	}

	public void RemoveRoot()
	{
		Debug.Assert(_count > 0);

		Node[] nodes = _nodes;
		Int32 lastIndex = --_count;
		if (lastIndex > 0)
			this.SiftDown(parentIndex: 0, nodes[lastIndex]);
		nodes[lastIndex] = default;
	}

	public void ReplaceRoot(TElement element, TKey key)
	{
		Debug.Assert(_count > 0);
		this.SiftDown(parentIndex: 0, new Node(element, key));
	}

	private static Int32 GetParentIndex(Int32 childIndex)
	{
		Debug.Assert(childIndex > 0);
		return (childIndex - 1) >>> 1;
	}

	private static Int32 GetLeftChildIndex(Int32 parentIndex)
	{
		Debug.Assert(parentIndex >= 0);
		return parentIndex * 2 + 1;
	}

	private void SiftDown(Int32 parentIndex, Node node)
	{
		Debug.Assert(parentIndex >= 0);
		Debug.Assert(parentIndex < _count);

		Comparison<TKey> comparison = _comparison;
		Node[] nodes = _nodes;
		Int32 count = _count;
		Int32 leftChildIndex;

		while ((leftChildIndex = GetLeftChildIndex(parentIndex)) < count)
		{
			Int32 rightChildIndex = leftChildIndex + 1;
			Int32 minChildIndex = rightChildIndex < count &&
				comparison(nodes[leftChildIndex].Key, nodes[rightChildIndex].Key) > 0
					? rightChildIndex
					: leftChildIndex;
			Node minChild = nodes[minChildIndex];
			if (comparison(node.Key, minChild.Key) <= 0)
				break;
			nodes[parentIndex] = minChild;
			parentIndex = minChildIndex;
		}

		nodes[parentIndex] = node;
	}
}
