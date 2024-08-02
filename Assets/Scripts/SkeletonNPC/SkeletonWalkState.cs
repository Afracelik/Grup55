using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonWalkState : StateMachineBehaviour

{
    NavMeshAgent skeleton;
    Transform player;
    float distance;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        skeleton = animator.GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        skeleton.speed = 5f;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        skeleton.SetDestination(player.position);
        //animator.transform.LookAt(player);

        distance = Vector3.Distance(player.position, animator.transform.position);

        if (distance <= 3.5f)
        {
            animator.SetBool("isAttacking", true);
        }
        if (distance >= 10f)
        {
            animator.SetBool("isWalking", false);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        skeleton.SetDestination(animator.transform.position);
    }

}

