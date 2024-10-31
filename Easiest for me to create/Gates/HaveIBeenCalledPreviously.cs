using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HaveIBeenCalledPreviously : MonoBehaviour
{
    [SerializeField] private UnityEvent IfHaveBeenCalled;
    [SerializeField] private UnityEvent IfHaveNotBeenCalled;

    bool haveIBeenCalled;

    public void Trigger()
    {
        if (haveIBeenCalled == true)
        {
            IfHaveBeenCalled?.Invoke();
        }
        else
        {
            IfHaveNotBeenCalled?.Invoke();
            haveIBeenCalled = true;
        }
    }
}