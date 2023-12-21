

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Illumination : MonoBehaviour
{
    bool isActive;

    Light L;

    // Start is called before the first frame update
    void Start()
    {
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
