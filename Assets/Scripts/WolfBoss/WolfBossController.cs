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
    public bool beingAttacked2;

    public float time = 0;

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

        time++;

        if (time >= 30)
        {
            beingAttacked = firstPersonController.attacking;

            if (health > 0)
            {
                if (beingAttacked && beingAttacked2)
                {
                    takeDamage(10);
                }
            }

            time = 0;
        }

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
            beingAttacked2 = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Axe")
        {
            beingAttacked2 = false;
        }
    }
}
