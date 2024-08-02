using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class IdleStateGoblin : StateMachineBehaviour
{
    private GameObject player;
    private Transform playerTransform;
    private float detectionRange = 50f;  // NPC'nin hareket etmeye baþlayacaðý mesafe

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
            // Ana karakter ile NPC arasýndaki mesafeyi hesapla
            float distance = Vector3.Distance(animator.transform.position, playerTransform.position);

            // Eðer mesafe detectionRange'den küçükse NPC'yi hareket ettir ve animasyonu deðiþtir
            if (distance < detectionRange)
            {
                // Animator'daki parametreyi "isWalking" olarak deðiþtir
                animator.SetBool("isWalking", true);
            }
            else
            {
                // Mesafe detectionRange'den büyükse, NPC'yi durdur ve animasyonu "Idle" olarak deðiþtir
                animator.SetBool("isWalking", false);
            }
        }
    }
}

