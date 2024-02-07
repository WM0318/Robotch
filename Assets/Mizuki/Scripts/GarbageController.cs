using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageController : MonoBehaviour
{
    [Header("Status")]
    [Tooltip("プレイヤーのブーストが溜まる量")]
    [SerializeField] float chargeBoost = 2.0f;
    [Tooltip("吸い込み力")]
    [SerializeField] float vacuumPower = 20.0f;
    [Tooltip("完全に吸い込まれる範囲")]
    [SerializeField] float eraseRange = 0.1f;

    PlayerController player;
    GameManager manager;

    float elapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        manager = GameObject.FindGameObjectWithTag("Player").GetComponent<GameManager>();
    }

    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Vacuum") && !player.isFullyGarbage)
        {
            elapsedTime += Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, other.transform.position, Mathf.Pow(1.0f, elapsedTime) / vacuumPower);

            if (-eraseRange < Vector3.Distance(transform.position, other.transform.position) && Vector3.Distance(transform.position, other.transform.position) < eraseRange) 
            {
                player.boostCharge = chargeBoost;
                player.addGarbage = 1.0f;

                manager.CheckProgress();

                gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Vacuum")) elapsedTime = 0.0f;
    }
}
