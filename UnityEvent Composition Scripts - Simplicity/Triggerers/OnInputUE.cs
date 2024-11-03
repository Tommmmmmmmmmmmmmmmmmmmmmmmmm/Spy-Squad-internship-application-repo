using UnityEngine.Events;
using UnityEngine;

public class OnInputUE : MonoBehaviour
{
    public KeyCode key;
    public UnityEvent pressed;

    private void Update()
    {
        if (Input.GetKeyDown(key)) pressed.Invoke();
    }
}