using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IPoolable
{
    public event Action<IPoolable> Released;

    public void Release()
    {
        Released?.Invoke(this);
    }
}