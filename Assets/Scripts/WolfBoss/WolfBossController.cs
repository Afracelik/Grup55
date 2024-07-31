using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfBossController : MonoBehaviour
{
    public int health = 100;
    public Animator animator;
    public Animator animator_player;

    public GameObject player;
    public FirstPersonController firstPersonController;

    public bool beingAttacked;
    public bool beingAttackedArea;

    public bool attackToPlayer;
    public bool attackToPlayerArea;


    // Start is called before the first frame update
    void Start()
    {
        firstPersonController = player.GetComponent<FirstPersonController>();
        animator = GetComponent<Animator>();
        animator_player = player.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        beingAttacked = firstPersonController.attackingWithHitPoint;
        if (health > 0)
        {
            if (beingAttacked && beingAttackedArea)
            {
                takeDamage(10);
            }
        }

        if (firstPersonController.currentHealth > 0)
        {
            print(attackToPlayer);
            if (attackToPlayer && attackToPlayerArea)
            {
                firstPersonController.ApplyDamage(5);
            }
        }

        attackToPlayer = false;

    }

    public void takeDamage(int damage)
    {
        health -= damage;
        
        if (health <= 0)
        {
            animator.SetTrigger("die");
        } else
        {
            animator.SetTrigger("damage");
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        //
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Axe")
        {
            beingAttackedArea = true;
        }

        if (other.tag == "Player")
        {
            attackToPlayerArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Axe")
        {
            beingAttackedArea = false;
        }

        if (other.tag == "Player")
        {
            attackToPlayerArea = false;
        }
    }

    public void TriggerEvent()
    {
        attackToPlayer = true;
    }

    public void ResetEvent()
    {
        attackToPlayer = false;
    }
}
