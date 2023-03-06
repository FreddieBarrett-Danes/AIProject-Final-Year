using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClubListener : MonoBehaviour
{
    private RedBB rBB;

    private void Start()
    {
        rBB = gameObject.transform.parent.GetComponent<RedBB>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Blue"))
        {
            other.gameObject.GetComponent<BlueBB>().health--;
        }
    }
}

