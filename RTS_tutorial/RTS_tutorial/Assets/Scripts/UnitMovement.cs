using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour
{
    Camera cam;
    NavMeshAgent agent;
    public LayerMask ground;

    public bool isLocalPlayer = false;
    public bool isCommandedToMove;
    
    DirectionIndicator directionIndicator;
    private void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();

        directionIndicator = GetComponent<DirectionIndicator>();
    }
    private void Update()
    {
        if (!isLocalPlayer) return; 
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
            {
                isCommandedToMove = true;

                Vector3 point = hit.point;
                string posString = $"{point.x:F2},{point.y:F2},{point.z:F2}";

                string userId = GameManager.userId;
                NetworkManager.Instance.SendTlpMessage(0x02, $"{userId} {posString}");

                agent.SetDestination(point);
                directionIndicator?.DrawLine(hit);
            }
        }

        if (agent.isOnNavMesh && (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance))
        {
            isCommandedToMove = false;
        }

    }
}
