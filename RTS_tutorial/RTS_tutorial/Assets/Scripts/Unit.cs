using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    private int unitHealth;
    public int unitMaxHealth;
    public HealthTracker healthTracker;

    Animator animator;
    NavMeshAgent navMeshAgent;
    void Start()
    {
        UnitSelectionManager.Instance.allUnitsList.Add(gameObject);

        unitHealth = unitMaxHealth;
        UpdateHealthUI();

        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }


    private void OnDestroy()
    {
        UnitSelectionManager.Instance.allUnitsList.Remove(gameObject);
    }
    private void UpdateHealthUI()
    {
        healthTracker.UpdateSliderValue(unitHealth, unitMaxHealth);

        if (unitHealth <= 0)
        {
            // Dying Logic

            // Destruction or Dying Animation

            // Dying Sound Effect
            Destroy(gameObject);
        }

    }
    internal void TakeDamage(int damageInInflict)
    {
        unitHealth -= damageInInflict;
        UpdateHealthUI();
    }

    private void Update()
    {
        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }
}
