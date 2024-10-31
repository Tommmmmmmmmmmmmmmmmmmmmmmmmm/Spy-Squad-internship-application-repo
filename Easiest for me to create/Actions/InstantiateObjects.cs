using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateObjects : MonoBehaviour
{
    [SerializeField] private GameObject[] objects; 
    [SerializeField] private bool instantiateAsChild; 
    [SerializeField] private float radius = 5; 
    [SerializeField] private int numberToInstantiate = 5; 

   public void Trigger()
    {
        for (int i = 0; i < numberToInstantiate; i++)
        {
            Vector2 pos = Random.insideUnitCircle * radius;

            Vector3 newPos = transform.position + new Vector3(pos.x, 0, pos.y);

            int r = Random.Range(0, objects.Length);

            GameObject go = Instantiate(objects[r], newPos, Quaternion.identity, transform);

            // its a bit jank but this ensures the object is being instantiated in the same scene as the object this is on - important for additive scene workflows
            if(!instantiateAsChild) go.transform.parent = null;

            go.transform.localScale = objects[r].transform.localScale;
        }
    }
}