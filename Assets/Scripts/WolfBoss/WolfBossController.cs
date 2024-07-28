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

        //bool isAttack1Playing = animator_player.GetCurrentAnimatorStateInfo(0).IsName("Attack1");


        time++;

        // E�er frame sayac� belirli bir de�ere ula�t�ysa
        if (time >= 30)
        {
            // Ata�� kontrol et
            beingAttacked = firstPersonController.attacking;

            // Di�er kontrol ve i�lemleri yap
            if (beingAttacked && beingAttacked2)
            {
                takeDamage(2);
            }

            // Frame sayac�n� s�f�rla
            time = 0;
        }


        //beingAttacked = firstPersonController.attacking;
        //Debug.Log(beingAttacked);

        //time = 0;
        //while (time < 30)
        //{
        //    time += 1;
        //    Debug.Log(time);
        //}


        //if (health > 0)
        //{
        //    if (beingAttacked && beingAttacked2)
        //    {
        //        takeDamage(2);
        //    }
        //}
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
        if (other.tag == "Axe")
        {
            beingAttacked2 = true;
            Debug.Log("�arp��ma");
        } else
        {
            beingAttacked2 = false;
            Debug.Log("�arp��ma yoook");

        }
    }
}
