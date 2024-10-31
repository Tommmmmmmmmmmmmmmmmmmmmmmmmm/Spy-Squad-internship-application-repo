using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using NaughtyAttributes;

public class DestroyObjectsInX : MonoBehaviour
{
    [SerializeField] private bool triggerOnStart;

    [SerializeField] private GameObject[] objects;

    [SerializeField] private bool randomiseCountdown = true;
    [HideIf("randomiseCountdown")]
    [SerializeField] private float countdown;
    [ShowIf("randomiseCountdown")]
    [SerializeField] private Vector2 randomCountdown;

    [SerializeField] private UnityEvent onDestroy;

    void Start()
    {
        if (triggerOnStart) Trigger();
    }

    private void Trigger() => StartCoroutine(DoTrigger());

    private IEnumerator DoTrigger()
    {
        yield return new WaitForSeconds(randomiseCountdown ? randomCountdown.x + Random.Range(-randomCountdown.y, randomCountdown.y) : countdown);

        onDestroy.Invoke();

        for (int i = 0; i < objects.Length; i++)
        {
            Destroy(objects[i]);
        }
    }
}