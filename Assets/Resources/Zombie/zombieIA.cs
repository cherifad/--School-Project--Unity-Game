using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zombieIA : MonoBehaviour
{
    private Vector3 PointDepart;
    private float DistancePointDepart;

    public Transform Player;//position du joueur
    private float Distance;//distance zombie/joueur

    public float RayonAction = 8;
    public float RayonAttaque = 0.8f;

    // Cooldown des attaques
    public float attackRepeatTime = 1;
    private float attackTime;
    public float degats = 10;


    //
    private float coolDownAttaque;

    private UnityEngine.AI.NavMeshAgent agent;
    private Animator playerAnimator;

    // Vie de l'ennemi
    public float enemyHealth;
    private bool isDead = false;

    // loots de l'ennemi
    public GameObject[] loots;

    void Start()
    {
        agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        playerAnimator = GetComponent<Animator>();
        attackTime = Time.time;
        PointDepart = transform.position;
    }

    void Update()
    {

        if (!isDead)
        {
            //Debug.Log("!isdead");

            Player = GameObject.Find("Player").transform;//localisation du personnage
            Distance = Vector3.Distance(Player.position, transform.position);//distance entre zombie et personnage
            DistancePointDepart = Vector3.Distance(PointDepart, transform.position);//distance entre le zombie et son point de départ

            /*if (Distance > RayonAction && DistancePointDepart <= 1)//comportement de base du zombie
            {
                playerAnimator.SetBool("pointDepart", true);
            }
            else
                playerAnimator.SetBool("pointDepart", false);*/

            if (Distance < RayonAction && Distance > RayonAttaque)//Perso dans la zone de détection du zombie
            {
                agent.destination = Player.position;
                playerAnimator.SetBool("poursuite", true);
            }
            else
                playerAnimator.SetBool("poursuite", false);

            if (Distance < RayonAttaque)//le zombie est est dans le rayon d'attaque
            {
                agent.destination = transform.position;//pour que le zombie resten en place

                if (coolDownAttaque > 0)
                    coolDownAttaque -= Time.deltaTime;
                if (coolDownAttaque < 0)
                    coolDownAttaque = 0;

                if (coolDownAttaque == 0)
                {
                    //Debug.Log("Attaque");
                    HealthBar.SetHealthBarValue(HealthBar.GetHealthBarValue() - 0.1f);
                    coolDownAttaque = 2;
                }
                //Debug.Log(coolDownAttaque);

                playerAnimator.SetBool("attaquer", true);
            }
            else
                playerAnimator.SetBool("attaquer", false);

            if (Distance > RayonAction && DistancePointDepart > 1)//on s'est trop éloigné du zombie
            {
                agent.destination = PointDepart;
                playerAnimator.SetBool("goBack", true);
            }
            else
                playerAnimator.SetBool("goBack", false);

        }
    }
}
