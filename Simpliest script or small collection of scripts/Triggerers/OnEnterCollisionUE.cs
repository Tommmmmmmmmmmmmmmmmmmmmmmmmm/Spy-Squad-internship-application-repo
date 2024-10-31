using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class OnEnterCollisionUE : MonoBehaviour
{
    [SerializeField] private bool CheckForTag;
    [ShowIf("CheckForTag")]
    [SerializeField] private string Tag;

    public UnityEvent OnTriggerEnterUE;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (CheckForTag && collision.gameObject.CompareTag(Tag)) OnTriggerEnterUE.Invoke();
        else OnTriggerEnterUE.Invoke();
    }
}