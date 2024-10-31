using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceWithOtherObjects : MonoBehaviour
{
    [SerializeField] private GameObject[] objects;
    [SerializeField] private bool instantiateAsChild;

    public void Trigger()
    {
        int i = Random.Range(0, objects.Length);

        if (objects[i] != null)
        {
            GameObject newObj = Instantiate(objects[i], transform.position, transform.rotation, transform.parent);
            if (!instantiateAsChild) newObj.transform.parent = null;
        }

        Destroy(gameObject);
    }
}