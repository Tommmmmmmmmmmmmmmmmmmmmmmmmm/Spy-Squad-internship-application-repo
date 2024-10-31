using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnUpdateUE : MonoBehaviour
{
    public UnityEvent OnUpdate;

    private void Update() => OnUpdate?.Invoke();
}