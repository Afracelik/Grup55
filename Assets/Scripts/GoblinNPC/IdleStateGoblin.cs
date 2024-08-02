using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class IdleStateGoblin : StateMachineBehaviour
{
    private GameObject player;
    private Transform playerTransform;
    private float detectionRange = 50f;  // NPC'nin hareket etmeye ba�layaca�� mesafe

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Oyuncuyu sahnede bul
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player object not found in the scene. Make sure the player has the 'Player' tag.");
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerTransform != null)
        {
            // Ana karakter ile NPC aras�ndaki mesafeyi hesapla
            float distance = Vector3.Distance(animator.transform.position, playerTransform.position);

            // E�er mesafe detectionRange'den k���kse NPC'yi hareket ettir ve animasyonu de�i�tir
            if (distance < detectionRange)
            {
                // Animator'daki parametreyi "isWalking" olarak de�i�tir
                animator.SetBool("isWalking", true);
            }
            else
            {
                // Mesafe detectionRange'den b�y�kse, NPC'yi durdur ve animasyonu "Idle" olarak de�i�tir
                animator.SetBool("isWalking", false);
            }
        }
    }
}

