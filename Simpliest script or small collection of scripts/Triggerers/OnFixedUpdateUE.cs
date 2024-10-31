using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnFixedUpdateUE : MonoBehaviour
{
    public UnityEvent OnFixedUpdate;

    private void FixedUpdate() => OnFixedUpdate?.Invoke();
}