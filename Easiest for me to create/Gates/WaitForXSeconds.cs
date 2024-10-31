using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

public class WaitForXSeconds : MonoBehaviour
{
    [SerializeField] private bool startWaitOnStart;

    [SerializeField] private bool randomiseCountdown = true;
    [HideIf("randomiseCountdown")]
    [SerializeField] private float countdown;
    [ShowIf("randomiseCountdown")]
    [SerializeField] private Vector2 randomCountdown;

    public UnityEvent OnWaitEnd;

    private void Start()
    {
        if (startWaitOnStart) StartWait();
    }

    public void StartWait() => StartCoroutine(Wait());

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(randomiseCountdown ? randomCountdown.x + Random.Range(-randomCountdown.y, randomCountdown.y) : countdown);

        OnWaitEnd?.Invoke();
    }
}