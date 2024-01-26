using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageController : MonoBehaviour
{
    [SerializeField] float chargeBoost = 2.0f;
    [SerializeField] float vacuumPower = 20.0f;
    [SerializeField] float eraseRange = 0.1f;

    float elapsedTime;

    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Vacuum"))
        {
            elapsedTime += Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, other.transform.position, Mathf.Pow(1.0f, elapsedTime) / vacuumPower);

            if (-eraseRange < Vector3.Distance(transform.position, other.transform.position) && Vector3.Distance(transform.position, other.transform.position) < eraseRange) 
            {
                other.GetComponentInParent<PlayerController>().boostCharge = chargeBoost;

                other.GetComponentInParent<GameManager>().CheckProgress();

                gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Vacuum")) elapsedTime = 0.0f;
    }
}
