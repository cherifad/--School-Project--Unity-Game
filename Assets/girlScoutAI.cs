using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class girlScoutAI : MonoBehaviour
{
    // Distance entre le joueur et l'ennemi
    private float Distance;

    // Distance entre l'ennemi et sa position de base
    private float DistanceBase;
    private Vector3 basePositions;

    // Cible de l'ennemi
    public Transform Target;

    //Distance de poursuite
    public float chaseRange = 10;

    // Portée des attaques
    public float attackRange = 2.2f;

    // Cooldown des attaques
    public float attackRepeatTime = 1;
    private float attackTime;

    // Montant des dégâts infligés
    public float TheDammage;

    // Agent de navigation
    private UnityEngine.AI.NavMeshAgent agent;

    // Animations de l'ennemi
    private Animation animations;
    private Animator playerAnimator;

    // Vie de l'ennemi
    public float enemyHealth;
    private bool isDead = false;

    // loots de l'ennemi
    public GameObject[] loots;

    void Start()
    {
        agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        animations = gameObject.GetComponent<Animation>();
        playerAnimator = GetComponent<Animator>();
        attackTime = Time.time;
        basePositions = transform.position;
    }

    void Update()
    {

        if (!isDead)
        {
            Debug.Log("!isdead");

            // On cherche le joueur en permanence
            Target = GameObject.Find("Principal").transform;

            // On calcule la distance entre le joueur et l'ennemi, en fonction de cette distance on effectue diverses actions
            Distance = Vector3.Distance(Target.position, transform.position);

            // On calcule la distance entre l'ennemi et sa position de base
            DistanceBase = Vector3.Distance(basePositions, transform.position);

            // Quand l'ennemi est loin = idle
            if (Distance > chaseRange && DistanceBase <= 1)
            {
                idle();
            }

            // Quand l'ennemi est proche mais pas assez pour attaquer
            if (Distance < chaseRange && Distance > attackRange)
            {
                //chase();
                agent.destination = Target.position;
                playerAnimator.SetBool("isChasing", true);
            }
            else
                playerAnimator.SetBool("isChasing", false);

            // Quand l'ennemi est assez proche pour attaquer
            /*if (Distance < attackRange)
            {
                attack();
            }*/

            // Quand le joueur s'est échappé
            if (Distance > chaseRange && DistanceBase > 1)
            {
                BackBase();
                playerAnimator.SetBool("goBack", true);
            }
            else
                playerAnimator.SetBool("goBack", false);

        }
    }

    // poursuite
/*    void chase()
    {
        //playerAnimator.SetBool("isChasing", true);
        agent.destination = Target.position;
    }*/

    // Combat
    /*void attack()
    {
        // empeche l'ennemi de traverser le joueur
        agent.destination = transform.position;

        //Si pas de cooldown
        if (Time.time > attackTime)
        {
            animations.Play("hit");
            Target.GetComponent<PlayerInventory>().ApplyDamage(TheDammage);
            Debug.Log("L'ennemi a envoyé " + TheDammage + " points de dégâts");
            attackTime = Time.time + attackRepeatTime;
        }
    }*/

    // idle
    void idle()
    {
        animations.Play("Z_Idle");
    }

    /*public void ApplyDammage(float TheDammage)
    {
        if (!isDead)
        {
            enemyHealth = enemyHealth - TheDammage;
            print(gameObject.name + "a subit " + TheDammage + " points de dégâts.");

            if (enemyHealth <= 0)
            {
                Dead();
            }
        }
    }*/

    // Retour a la base si le joueur s'est échappé et si notre ennemi est trop loin de sa base
    public void BackBase()
    {
        animations.Play("Z_Walk1_InPlace");
        agent.destination = basePositions;
    }

    /*public void Dead()
    {
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        isDead = true;
        animations.Play("die");

        // apparition du loot
        int randomNumber = Random.Range(0, loots.Length);
        GameObject finalLoot = loots[randomNumber];
        Instantiate(finalLoot, transform.position, transform.rotation);

        Destroy(transform.gameObject, 5);
    }*/
}