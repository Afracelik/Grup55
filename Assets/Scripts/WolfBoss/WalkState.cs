using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkState : StateMachineBehaviour
{
    float timer;
    List<Transform> wayPoints = new List<Transform>();
    NavMeshAgent wolf;

    float distance;
    float chaseRange = 8;

    Transform player;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        wolf = animator.GetComponent<NavMeshAgent>();
        wolf.speed = 1.5f;

        player = GameObject.FindGameObjectWithTag("Player").transform;


        timer = 0;
        GameObject wpoints = GameObject.FindGameObjectWithTag("WayPoints");

        foreach(Transform t in wpoints.transform)
        {
            wayPoints.Add(t);
        }

        wolf.SetDestination(wayPoints[Random.Range(0, wayPoints.Count)].position);

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;
        if (timer > 10)
        {
            animator.SetBool("isWalking", false);
        }

        if(wolf.remainingDistance <= wolf.stoppingDistance)
        {
            wolf.SetDestination(wayPoints[Random.Range(0, wayPoints.Count)].position);
        }

        distance = Vector3.Distance(player.position, animator.transform.position);
        if (distance < chaseRange)
        {
            animator.SetBool("isChasing", true);

        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        wolf.SetDestination(wolf.transform.position);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
