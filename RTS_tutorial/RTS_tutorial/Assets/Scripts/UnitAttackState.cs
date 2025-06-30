using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitAttackState : StateMachineBehaviour
{
    NavMeshAgent agent;
    AttackController attackController;

    public float stopAttackingDistance = 7.5f;

    public float attackRate = 2f;
    private float attackTimer;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        attackController = animator.GetComponent<AttackController>();
        attackController.SetAttackMaterial();
        attackController.muzzleEffect.gameObject.SetActive(true);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackController.targetToAttack != null && animator.transform.GetComponent<UnitMovement>().isCommandedToMove == false) 
        {
            LookAtTarget();

            // agent.SetDestination(attackController.targetToAttack.position);
            
            if (attackTimer <= 0)
            {
                Attack();
                attackTimer = 1f/attackRate;
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }

            // Should unit still attack
            float distanceFromTarget = Vector3.Distance(attackController.targetToAttack.position, animator.transform.position);
            if (distanceFromTarget > stopAttackingDistance || attackController.targetToAttack == null)
            {
                agent.SetDestination(animator.transform.position);
                animator.SetBool("isAttacking", false); // Move to Attacking state
            }

        } 
        else
        {
            animator.SetBool("isAttacking", false); // Move to Attacking state
        }

    }

    private void Attack()
    {
        var damageInInflict = attackController.unitDamage;

        SoundManager.Instance.PlayInfantryAttackSound();

        // Actually Attaqck Unit
        attackController.targetToAttack.GetComponent<Unit>().TakeDamage(damageInInflict);

    }

    private void LookAtTarget()
    {
        Vector3 direction = attackController.targetToAttack.position - agent.transform.position;
        agent.transform.rotation = Quaternion.LookRotation(direction);

        var yRotation = agent.transform.eulerAngles.y;
        agent.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        attackController.muzzleEffect.gameObject.SetActive(false);
    }
}
