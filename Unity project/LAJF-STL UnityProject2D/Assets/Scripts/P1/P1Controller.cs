﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using STL2.Events;

public class P1Controller : MonoBehaviour
{
    public enum Player1Input
    {
        Horizontal,
        Jump,
        Attack
    };
    #region INSPECTOR
    public P1Stats playerStats;
    public IntEvent playerHPEvent;

    public Rigidbody2D rb;//declares variable to give the player gravity and the ability to interact with physics

    /* controls */
    public KeyCode left, right, jump, attackRanged, attackMelee;

    [SerializeField]
    private SpriteRenderer spriteRenderer;
    
    //White and default materials
    private Material matDefault; 
    public Material matWhite;

    public ParticleSystem deathLeadUp, deathExplosion;
    public HealthBar healthBar;
    public Transform particlePoint;


    /* Player Status */
    public int currentHitPoints; // current amount of HP
    public bool attackIsOnCooldown; // is the attack on cooldown
    public bool isGrounded; //it's either true or false if the player is on the ground

    private Vector2 moveDirection;

    public GameObject projectile;

    public float rangedAttackCooldownTimer = 0;
    public float rangedCooldownMaxTime = 0.2f;
    private bool justUsedRangedAttack = false;

    public LayerMask obstacles;

    public float projectileSpeed;


    public ChoiceCategory runtimeChoices;

    public List<PlayerItems> playerItems = new List<PlayerItems>();

    private AudioList _audioList;
    public AudioList audioList { get { return _audioList; } }
    #endregion

    void Awake()
    {
        moveDirection = Vector2.right;
    }

    private void Start()
    {
        _audioList = FindObjectOfType<AudioList>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        matDefault = spriteRenderer.material;
        
    }

    private void Update()
    {
        #region UpdateCooldowns
        if (justUsedRangedAttack)
        {
            rangedAttackCooldownTimer += Time.deltaTime;
            if (rangedAttackCooldownTimer >= rangedCooldownMaxTime)
            {
                justUsedRangedAttack = false;
                rangedAttackCooldownTimer = 0;
            }
        }
        #endregion UpdateCooldowns





        #region UpdateSprites
        spriteRenderer.flipX = moveDirection.x > 0;
        #endregion UpdateSprites
    }




    public void MeleeAttack()
    {

    }

    public void RangedAttack()
    {
        GameObject instance = Instantiate(projectile, transform.position + (((Vector3)moveDirection) * 0.2f), Quaternion.identity);
        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        //rb.velocity = moveDirection * projectileSpeed;
        rb.AddForce(moveDirection * projectileSpeed, ForceMode2D.Impulse);


        Projectile projInstance = instance.GetComponent<Projectile>();
        projInstance.damage = 10;


        justUsedRangedAttack = true;
    }

    public void TakeDamage(int damage)
    {
        currentHitPoints -= damage;
        playerHPEvent.Raise(currentHitPoints);
        if (currentHitPoints > 0) // Show damage effect
        {
            DamageAnimation();
            audioList.PlayWithVariablePitch(audioList.hurt);
        }
        else // Trigger death effect
        {
            DeathAnimation();
            audioList.PlayWithVariablePitch(audioList.deathHero);
        }
    }

    private bool canPlayerAttack()
    {
        //Can have many more conditionals changing this in the future:
        return !justUsedRangedAttack;
    }

    public void ReceiveInput(Player1Input input, float value)
    {
        switch (input)
        {
            case Player1Input.Attack:
                if (!canPlayerAttack())
                {
                    return;
                }

                if (playerStats.rangedAttacks)
                {
                    RangedAttack();
                    return;
                }

                if (playerStats.meleeAttacks)
                {
                    MeleeAttack();
                    return;
                }

                break;
            case Player1Input.Horizontal:


                rb.velocity = new Vector2(playerStats.moveSpeed * value, rb.velocity.y); //if we press the key that corresponds with KeyCode left, then we want the rigidbody to move to the left
                moveDirection = (value > 0.1f) ? Vector2.right :
                    (value < -0.1f) ? Vector2.left : moveDirection;

                if (isPlayerCloseToWall(1.5f))
                {
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }

                break;
            case Player1Input.Jump:
                if (isGrounded)
                {
                    rb.AddForce(playerStats.jumpForce * Vector2.up, ForceMode2D.Impulse);
                    audioList.PlayWithVariablePitch(audioList.jump);
                }
                break;


        }


    }

    private bool isPlayerCloseToWall(float range)
    {
        //Has to collisioncheck for walls/ground after every round of movement-input.
        //Check if there is a wall on the side the player is moving:
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, range, obstacles);
        // If it hits something...
        if (hit.collider != null && (hit.transform.CompareTag("Wall") || hit.transform.CompareTag("Ground")))
        {
            return true;
        }

        return false;
    }

    public void UpdatePlayerStats()
    {
        List<PlayerItems> chosenItems = runtimeChoices.playerItems;

        foreach (PlayerItems playerItem in chosenItems)
        {
            //Skip the item if it's already in the players possession:
            if (playerItems.Contains(playerItem))
            {
                continue;
            }
            playerStats.maxHitPoints += playerItem.healthModifier;
            currentHitPoints += playerItem.healthModifier;

            playerStats.moveSpeed += playerItem.speedModifier;

            playerStats.baseAttackDamage += playerItem.damageModifier;
        }

        playerItems.AddRange(chosenItems.Except(playerItems)); //Add new items to playeritems
        Debug.Log("Updated runtime playerstats with new item!");
        
        //Strictly speaking only necessary if playerHP actually changed here, but for good measure:
        playerHPEvent.Raise(currentHitPoints);
    }

    public void DamageAnimation()
    {
        // Add white flash
        spriteRenderer.material = matWhite;
        Invoke("ResetMaterial", 0.1f);
    }

    void ResetMaterial()
    {
        spriteRenderer.material = matDefault;
    }

    public void DeathAnimation() // Add Particle Burst
    {
        Destroy(rb);
        Instantiate(deathLeadUp, particlePoint.position, particlePoint.rotation);
        healthBar.transform.parent = null;
        Invoke("DeathExplode", 1);
    }

    public void DeathExplode() // Add Particle Burst
    {
        Instantiate(deathExplosion, particlePoint.position, particlePoint.rotation);
        Destroy(gameObject);

    }

}
