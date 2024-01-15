

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Illumination : MonoBehaviour
{
    public GameObject remotecontroller;
    public GameObject player;
    bool isActive;

    Light L;



    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");

        //Instantiate(remotecontroller, player.transform.
        //position, player.transform.rotation) as GameObject;
        L = GetComponent<Light>();

        isActive = L.isActiveAndEnabled;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isActive = !isActive;

            L.enabled = isActive;
        }
    }
}
