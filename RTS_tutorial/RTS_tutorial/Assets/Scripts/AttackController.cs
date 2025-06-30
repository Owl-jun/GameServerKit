using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    public Transform targetToAttack;

    public Material idleStateMaterial;
    public Material followStateMaterial;
    public Material attackStateMaterial;

    public bool isPlayer;

    public int unitDamage;

    public GameObject muzzleEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (isPlayer && other.CompareTag("Enemy") && targetToAttack == null)
        {
            targetToAttack = other.transform;
            NetworkManager.Instance.SendTlpMessage(0x03, "STOP " + PlayerManager.Instance.myPlayer.name + " " + targetToAttack.name);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (isPlayer && other.CompareTag("Enemy") && targetToAttack == null)
        {
            targetToAttack = other.transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (isPlayer && !other.CompareTag("Enemy") && other.CompareTag("Enemy") && targetToAttack != null)
        {
            targetToAttack = null;
            NetworkManager.Instance.SendTlpMessage(0x03,"START " + PlayerManager.Instance.myPlayer.name + " " + targetToAttack.name);
        }
    }

    public void SetIdleMaterial()
    {
        // GetComponent<Renderer>().material = idleStateMaterial;
    }    
    public void SetFollowMaterial()
    {
        // GetComponent<Renderer>().material = followStateMaterial;
    }    
    public void SetAttackMaterial()
    {
        // GetComponent<Renderer>().material = attackStateMaterial;
    }

    private void OnDrawGizmos()
    {
        // Follow Distance / Area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f*0.2f);

        // Attack Distance / Area
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);

        // Stop Attack Distance / Area
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }


}
