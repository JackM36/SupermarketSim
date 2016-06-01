using UnityEngine;
using System.Collections;
using System;

public class Heap<T> where T: IHeapItem<T>
{
    T[] items;
    int itemsCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = itemsCount;
        items[itemsCount] = item;

        sortUp(item);
        itemsCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];
        itemsCount--;
        items[0] = items[itemsCount];
        items[0].HeapIndex = 0;

        sortDown(items[0]);
        return firstItem;
    }

    public void UpdateItem(T item)
    {
        sortUp(item);
    }

    void sortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                swapItems(item, parentItem);
            }
            else
            {
                break;
            }

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void sortDown(T item)
    {
        while (true)
        {
            int childIndexL = item.HeapIndex * 2 + 1;
            int childIndexR = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexL < itemsCount)
            {
                swapIndex = childIndexL;

                if (childIndexR < itemsCount)
                {
                    if (items[childIndexL].CompareTo(items[childIndexR]) < 0)
                    {
                        swapIndex = childIndexR;
                    }
                }

                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    swapItems(item, items[swapIndex]);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }

    public int Count
    {
        get
        {
            return itemsCount;
        }
    }

    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    void swapItems(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}