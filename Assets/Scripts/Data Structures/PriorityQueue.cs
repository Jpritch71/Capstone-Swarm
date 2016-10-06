using UnityEngine;
using System.Collections.Generic;
using System;

public class PriorityQueue<T, Key> where T : System.IComparable<T>, QueueType<Key>
{
	private HashSet<Key> membershipSet;
	public T[] values; //CHANGE THIS BACK TO PRIVATE AFTER TESTING
	private int heapSize;

    public static int id = 1;
    public int idA;
	
	public PriorityQueue(int size) 
	{
        idA = id;
        ++id;
		values = new T[size];
		heapSize = 0;
		membershipSet = new HashSet<Key> ();
	}

	public void ResetQueue()
	{
		heapSize = 0;
		membershipSet.Clear();
	}

	public T RemoveTop() 
	{
		try 
		{
			T temp = values[0];
			membershipSet.Remove(temp.key);
			values[0] = values[heapSize - 1];
			heapSize--;

			if(heapSize > 0)
				ClimpDownHeap(0);
			return temp;	
		}
		catch (System.IndexOutOfRangeException X) 
		{
			Debug.Log (X.Message + " Heap is Empty.");
			return default(T);
		}
	}

	public bool InsertElement(T elementIn)
	{
		try
		{
			if (heapSize == values.Length)
			{
				throw new System.OverflowException("Heap Overflow");
			}
			else 
			{
				heapSize++;
				values[heapSize - 1] = elementIn;
				ClimpUpHeap(heapSize - 1);

				membershipSet.Add(elementIn.key);
				return true;
			}
		}
		catch(System.OverflowException X)
		{
            SearchNode temp = elementIn as SearchNode;
            id = (int)(Convert.ToInt32(elementIn.key));
            Debug.Log(temp.PairedNode);
            Debug.Log(temp.key);
			Debug.Log(X.Message + " -- Look into Dynamic Expansion | Heap Size: " + Size);
            Debug.Break();
			return false;
		}
	} 

	public void ClimpUpHeap(int indexIn)
	{
		if (indexIn == 0)
			return;

		int parentX = GetParentIndex (indexIn);
		int contrast = values [indexIn].CompareTo (values [parentX]);

		if (contrast >= 0)
			return;

		T temp = values [parentX];
		values [parentX] = values [indexIn];
		values [indexIn] = temp;
		ClimpUpHeap (parentX);
	}

	public void ClimpDownHeap(int indexIn)
	{
		if (indexIn >= heapSize)
			return;
		
		int childLeftX = GetLeftIndex (indexIn);
		int childRightX = GetRightIndex (indexIn);

		if (childLeftX >= heapSize || childRightX >= heapSize)
			return;

		int contrastLeft = values [indexIn].CompareTo (values [childLeftX]);
		int contrastRight = values [indexIn].CompareTo (values [childRightX]);

		T temp;
		if(values [childLeftX].CompareTo (values [childRightX]) <= 0)
		{
			if(contrastLeft > 0)
			{
				temp = values [childLeftX];
				values [childLeftX] = values [indexIn];
				values [indexIn] = temp;
				ClimpDownHeap (childLeftX);
			}
		}
		else
		{
			if(contrastRight >	 0)
			{
				temp = values [childRightX];
				values [childRightX] = values [indexIn];
				values [indexIn] = temp;
				ClimpDownHeap (childRightX);
			}
		}
	}
	
	public bool IsEmpty() 
	{
		return (heapSize == 0);
	}
	
	private int GetLeftIndex(int nodeIndex) 
	{
		return 2 * nodeIndex + 1;
	}
	
	private int GetRightIndex(int nodeIndex) 
	{
		return 2 * nodeIndex + 2;
	}
	
	private int GetParentIndex(int nodeIndex) 
	{
		return (nodeIndex - 1) / 2;
	}

	public bool IsMember(T elementIn)
	{
		return membershipSet.Contains (elementIn.key);
	}

	public int Size
	{
		get { return heapSize; }
	}

	public T PeekTop() 
	{
		try 
		{
			return values [0];
		}
		catch (System.IndexOutOfRangeException X) 
		{
			Debug.Log (X.Message + " Heap is Empty.");
			return default(T);
		}
	}
}