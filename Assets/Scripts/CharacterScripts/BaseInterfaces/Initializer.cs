using UnityEngine;
using System.Collections;

public abstract class Initializer : MonoBehaviour
{
    void Awake()
    {
        InitAwake();
    }

    void Start()
    {
        InitStart();
    }

    protected abstract void InitAwake();
    protected abstract void InitStart();
}
