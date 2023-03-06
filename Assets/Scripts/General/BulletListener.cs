using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletListener : MonoBehaviour
{
    private GameObject[] redTeam;

    private void Start()
    {
        redTeam = GameObject.FindGameObjectsWithTag("Red");
    }
    private void OnTriggerEnter(Collider other)
    {

        foreach (GameObject red in redTeam)
        {
            if (other.gameObject == red && red != null)
            {

                other.gameObject.GetComponent<RedBB>().health--;
                Destroy(gameObject);
            }

        }


        if (other.gameObject.tag == "Cover")
        {
            Destroy(gameObject);
        }
    }
}
