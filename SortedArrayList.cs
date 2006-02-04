using System;
using System.Collections;

namespace BlueprintIT.Utils
{
	public class SortedArrayList: IList
	{
		private ArrayList list;
		private IComparer comparer = null;

		public SortedArrayList()
		{
			list = new ArrayList();
		}

		public SortedArrayList(ICollection collection)
		{
			list = new ArrayList(collection);
			list.Sort();
		}

		public SortedArrayList(IComparer comparer)
		{
			this.comparer=comparer;
			list = new ArrayList();
		}

		public SortedArrayList(ICollection collection, IComparer comparer)
		{
			this.comparer=comparer;
			list = new ArrayList(collection);
			list.Sort(comparer);
		}

		public void Reorder(IComparer comparer)
		{
			this.comparer=comparer;
			list.Sort(comparer);
		}

		#region IList members
		public bool IsFixedSize
		{
			get
			{
				return list.IsFixedSize;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return list.IsReadOnly;
			}
		}

		public object this[int pos]
		{
			get
			{
				return list[pos];
			}

			set
			{
				throw new NotSupportedException("Objects can only be added to the list");
			}
		}

		public int Add(object value)
		{
			int pos = list.BinarySearch(value,comparer);
			if (pos<0)
			{
				pos=~pos;
			}
			list.Insert(pos,value);
			return pos;
		}

		public void Clear()
		{
			list.Clear();
		}

		public bool Contains(object value)
		{
			return list.Contains(value);
		}

		public int IndexOf(object value)
		{
			return list.IndexOf(value);
		}

		public void Insert(int index, object value)
		{
			throw new NotSupportedException("Objects can only be added to the list");
		}

		public void Remove(object value)
		{
			list.Remove(value);
		}

		public void RemoveAt(int pos)
		{
			list.RemoveAt(pos);
		}
		#endregion

		#region ICollection members
		public int Count
		{
			get
			{
				return list.Count;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return list.IsSynchronized;
			}
		}

		public object SyncRoot
		{
			get
			{
				return list.SyncRoot;
			}
		}

		public void CopyTo(Array array, int index)
		{
			list.CopyTo(array,index);
		}
		#endregion

		#region IEnumerable members
		public IEnumerator GetEnumerator()
		{
			return list.GetEnumerator();
		}
		#endregion
	}
}
