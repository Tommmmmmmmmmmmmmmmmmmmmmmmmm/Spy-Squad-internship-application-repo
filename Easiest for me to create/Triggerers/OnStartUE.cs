using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class OnStartUE : MonoBehaviour
{
    public UnityEvent OnStart;

    void Start() => OnStart.Invoke();
}