﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using STL2.Events;
using UnityEngine.UI;

public class P1Controller : MonoBehaviour
{
    public enum Player1Input
    {
        Horizontal,
        Jump,
        Attack,
        JumpHold,
        DoubleTapLeft,
        DoubleTapRight
    };
    #region INSPECTOR
    public P1Stats runtimePlayerStats;
    public IntEvent playerHPEvent;

    public Rigidbody2D rb;//declares variable to give the player gravity and the ability to interact with physics

    /* controls */
    public KeyCode left, right, jump, attackRanged, attackMelee;

    [SerializeField]
    public Sprite defaultSprite, playerSprite;
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

    [SerializeField]
    private float dropGravityModifier, baseGravity;
    [SerializeField]
    private bool isHoldingJump;

    private bool dashingLeft, dashingRight;
    private float dashTimer;
    [SerializeField]
    private float dashMaxTime = 0.3f;
    [SerializeField]
    private float dashSpeed;
    private bool dashOnCooldown;
    private bool hasShotgun = false;
    public bool hasGatlingGun = false;
    [SerializeField]
    [Range(0.1f, 0.9f)]
    private float gatlingSpeedMultiplier = 0.75f;

    [SerializeField]
    [Range(0.1f, 0.9f)]
    private float gatlingMovespeedMultiplier = 0.5f;

    private bool hasExplodingShots = false;

    [SerializeField]
    private Image shotgunImage;


    #endregion INSPECTOR

    void Awake()
    {
        _audioList = FindObjectOfType<AudioList>();
        moveDirection = Vector2.right;
        InitializePlayerStats(runtimeChoices.chosenHero); //Use the chosen stats to set baseline of this run.
    }

    private void Start()
    {


        spriteRenderer = GetComponent<SpriteRenderer>();
        matDefault = spriteRenderer.material;
    }



    private void Update()
    {
        //Cheat to enable weapons for testing:
        if (Input.GetKeyDown(KeyCode.Q))
        {
            hasShotgun = true;

            if (!hasGatlingGun)
            {
                hasGatlingGun = true;
                runtimePlayerStats.attackRate = runtimePlayerStats.attackRate * gatlingSpeedMultiplier;
                rangedCooldownMaxTime = runtimePlayerStats.attackRate;
            }



            hasExplodingShots = true;

            shotgunImage.gameObject.SetActive(true);
        }

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

        if (dashingLeft || dashingRight)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer > dashMaxTime)
            {
                dashingLeft = false;
                dashingRight = false;
                dashTimer = 0;
            }
        }
        if (dashOnCooldown)
        {
            if (isGrounded)
            {
                dashOnCooldown = false;
            }
        }
        #endregion UpdateCooldowns


        #region MovementModifying
        if (rb != null)
        {
            rb.gravityScale = isHoldingJump ? baseGravity : dropGravityModifier;
        }

        #endregion MovementModifying



        #region UpdateSprites
        spriteRenderer.flipX = moveDirection.x > 0;
        #endregion UpdateSprites
    }


    public void MeleeAttack()
    {

    }

    public void RangedAttack()
    {
        #region BaseLineAttack
        GameObject instance = Instantiate(projectile, transform.position + (((Vector3)moveDirection) * 0.2f), Quaternion.identity);
        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        rb.AddForce(moveDirection * projectileSpeed, ForceMode2D.Impulse);

        Projectile projInstance = instance.GetComponent<Projectile>();
        projInstance.damage = (int)runtimePlayerStats.baseAttackDamage;
        #endregion BaseLineAttack

        audioList.PlayWithVariablePitch(audioList.attack1);

        #region ModifiedAttacks
        if (hasShotgun)
        {
            int magicNumberExtraProjectileCount = 2;

            float offsetMagicNumber = 1.5f;
            for (int i = 0; i < magicNumberExtraProjectileCount; i++)
            {

                int sign = i % 2 == 0 ? 1 : -1;
                float yPositionOfShot = sign * offsetMagicNumber;
                Vector3 shotPosition = new Vector3(0, yPositionOfShot, 0);

                GameObject shotgunInstance = Instantiate(projectile, transform.position + (((Vector3)moveDirection) * 0.2f) + shotPosition, Quaternion.identity);
                Rigidbody2D rbShotgun = shotgunInstance.GetComponent<Rigidbody2D>();
                rbShotgun.AddForce(moveDirection * projectileSpeed, ForceMode2D.Impulse);

                Projectile shotgunProjInstance = shotgunInstance.GetComponent<Projectile>();
                shotgunProjInstance.damage = (int)runtimePlayerStats.baseAttackDamage;


                if (i % 2 == 0 && i != 0)
                {
                    offsetMagicNumber += 1.5f;
                }
            }
        }

        if (hasGatlingGun)
        {

        }



        #endregion ModifiedAttacks

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

    private bool CanPlayerAttack()
    {
        //Can have many more conditionals changing this in the future:
        return !justUsedRangedAttack;
    }

    public void ReceiveInput(Player1Input input, float value)
    {
        switch (input)
        {
            case Player1Input.Horizontal:


                rb.velocity = new Vector2(runtimePlayerStats.moveSpeed * value, rb.velocity.y); //if we press the key that corresponds with KeyCode left, then we want the rigidbody to move to the left
                moveDirection = (value > 0.1f) ? Vector2.right :
                    (value < -0.1f) ? Vector2.left : moveDirection;



                if (dashingLeft || dashingRight)
                {
                    float leftD, rightD;
                    leftD = dashingLeft ? 1 : 0;
                    rightD = dashingRight ? 1 : 0;
                    float dashDirection = rightD - leftD;
                    Debug.Log("DASH DIRECTION: " + dashDirection);
                    rb.velocity = new Vector2(dashDirection * dashSpeed, 0);
                }
                if (IsPlayerCloseToObstacle(1.5f))
                {

                    rb.velocity = new Vector2(0, rb.velocity.y);
                }

                if (hasGatlingGun && justUsedRangedAttack)
                {
                    rb.velocity = new Vector2(rb.velocity.x * gatlingMovespeedMultiplier, rb.velocity.y);
                }

                break;
            case Player1Input.Attack:
                if (!CanPlayerAttack())
                {
                    return;
                }

                if (runtimePlayerStats.rangedAttacks)
                {
                    RangedAttack();
                    return;
                }

                if (runtimePlayerStats.meleeAttacks)
                {
                    MeleeAttack();
                    return;
                }


                break;
            case Player1Input.Jump:

                if (isGrounded)
                {
                    audioList.PlayWithVariablePitch(audioList.jump);
                    rb.AddForce(runtimePlayerStats.jumpForce * Vector2.up, ForceMode2D.Impulse);
                }
                break;
            case Player1Input.JumpHold:
                if (value == 1)
                {
                    isHoldingJump = true;
                }
                else
                {
                    isHoldingJump = false;
                }
                break;

            case Player1Input.DoubleTapLeft:
                if (isGrounded || dashOnCooldown)
                {
                    return;
                }
                dashingLeft = true;
                dashOnCooldown = true;
                //dashTimer = 0;
                break;
            case Player1Input.DoubleTapRight:
                if (isGrounded || dashOnCooldown)
                {
                    return;
                }
                dashingRight = true;
                dashOnCooldown = true;
                //dashTimer = 0;
                break;

        }


    }

    private bool IsPlayerCloseToObstacle(float range)
    {
        //Has to collisioncheck for walls/ground after every round of movement-input.
        //Check if there is a wall on the side the player is moving:
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, range, obstacles);
        // If it hits something...
        if (hit.collider != null && (hit.transform.CompareTag("Wall") || hit.transform.CompareTag("Ground") || hit.transform.CompareTag("Cage")))
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
            runtimePlayerStats.maxHitPoints += playerItem.healthModifier;
            currentHitPoints += playerItem.healthModifier;

            runtimePlayerStats.moveSpeed += playerItem.speedModifier;

            runtimePlayerStats.baseAttackDamage += playerItem.damageModifier;


            //Weapontypes addons:
            switch (playerItem.weaponType)
            {
                case PlayerItems.WeaponType.None:
                    break;

                case PlayerItems.WeaponType.ExplodingShots:
                    hasExplodingShots = true;
                    shotgunImage.sprite = playerItem.itemSprite;
                    shotgunImage.gameObject.SetActive(true);
                    break;

                case PlayerItems.WeaponType.Gatlinggun:
                    hasGatlingGun = true;
                    runtimePlayerStats.attackRate = runtimePlayerStats.attackRate * gatlingSpeedMultiplier;
                    rangedCooldownMaxTime = runtimePlayerStats.attackRate;
                    shotgunImage.sprite = playerItem.itemSprite;
                    shotgunImage.gameObject.SetActive(true);
                    break;

                case PlayerItems.WeaponType.Shotgun:
                    hasShotgun = true;
                    shotgunImage.sprite = playerItem.itemSprite;
                    shotgunImage.gameObject.SetActive(true);
                    break;
            }
        }

        playerItems.AddRange(chosenItems.Except(playerItems)); //Add new items to playeritems
        Debug.Log("Updated runtime playerstats with new item!");

        //Strictly speaking only necessary if playerHP actually changed here, but for good measure:
        playerHPEvent.Raise(currentHitPoints);
    }

    public void InitializePlayerStats(P1Stats baselineStats)
    {
        runtimePlayerStats.baseAttackDamage = baselineStats.baseAttackDamage;
        runtimePlayerStats.maxHitPoints = baselineStats.maxHitPoints;
        runtimePlayerStats.moveSpeed = baselineStats.moveSpeed;

        runtimePlayerStats.attackRate = baselineStats.attackRate;
        rangedCooldownMaxTime = runtimePlayerStats.attackRate;

        runtimePlayerStats.jumpForce = baselineStats.jumpForce;
        runtimePlayerStats.rangedAttacks = baselineStats.rangedAttacks;


        runtimePlayerStats.meleeAttacks = baselineStats.meleeAttacks;
        if (runtimeChoices.chosenHero.characterSprite != null)
        {
            playerSprite = runtimeChoices.chosenHero.characterSprite;
        }
        else
        {
            playerSprite = defaultSprite;
        }
        GetComponent<SpriteRenderer>().sprite = playerSprite;



        currentHitPoints = baselineStats.startingHitPoints;
        playerHPEvent.Raise(currentHitPoints);
        //healthBar.VisualiseHealthChange(baselineStats.startingHitPoints);

        runtimeChoices.baselineItem = baselineStats.startItem;

        //Weapontypes addons:
        switch (baselineStats.startItem.weaponType)
        {
            case PlayerItems.WeaponType.None:
                //Do nothing
                break;

            case PlayerItems.WeaponType.ExplodingShots:
                hasExplodingShots = true;
                shotgunImage.sprite = baselineStats.startItem.itemSprite;
                shotgunImage.gameObject.SetActive(true);
                break;

            case PlayerItems.WeaponType.Gatlinggun:
                hasGatlingGun = true;
                shotgunImage.sprite = baselineStats.startItem.itemSprite;
                shotgunImage.gameObject.SetActive(true);
                break;

            case PlayerItems.WeaponType.Shotgun:
                hasShotgun = true;
                shotgunImage.sprite = baselineStats.startItem.itemSprite;
                shotgunImage.gameObject.SetActive(true);
                break;
        }
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
        ParticleSystem instance = Instantiate(deathLeadUp, particlePoint.position, particlePoint.rotation);
        healthBar.transform.parent = null;
        Destroy(instance.gameObject, instance.duration);
        Invoke("DeathExplode", 1);
    }

    public void DeathExplode() // Add Particle Burst
    {
        audioList.explosion.Play();

        ParticleSystem instance = Instantiate(deathExplosion, particlePoint.position, particlePoint.rotation);
        Destroy(gameObject);
        Destroy(instance.gameObject, instance.duration);
    }


}
