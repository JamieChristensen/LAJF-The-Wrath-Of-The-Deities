﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Experimental.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer))]
public class GodController : MonoBehaviour
{
    public GodInformation[] availableGods;

    [SerializeField]
    private ChoiceCategory runtimeChoices;
    [SerializeField]
    private GodInformation godInfo;

    [SerializeField]
    [Range(1, 3)]
    private int godNumber;

    [SerializeField]
    private Sprite sprite;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private TextMeshPro tmproName;

    private bool isEnabled;

    [SerializeField]
    private Sprite[] emotes;
    [SerializeField]
    private SpriteRenderer emoteRenderer;

    private float emoteDuration, emoteMaxTime;
    private bool isEmoting;

    [SerializeField]
    private SettingsScrObj gamesettings;

    // movement
    public float moveSpeed;
    public string horizontalAxis;
    public KeyCode horiRightKeyboardTestKey, horiLeftKeyboardTestKey;
    public KeyCode shoot;
    public KeyCode altShoot;
    public KeyCode controllerEmoteKey;

    /*
    public GameObject lightningPrefab;
    public Transform firePoint;
    private bool OnCooldown = false;
    private float timer = 0, cooldownTime = 15;
    */


    // general for attacks
    public bool inCombatMode = true; // the god can only attack if this is true 
    private GodInformation.AttackTypes attackType;
    public float minIntensity;
    public float maxIntensity;

    public TextMeshProUGUI readyForFire;

    // Lightning v2
    public GameObject lightningStrikePrefab;
    private bool canAttack = true;
    public float chargeTime = 2;
    //Laser beam
    public GameObject laserBeamPrefab;
    public float laserCooldown;
    private float laserCooldownTimer = 0;
    public int laserAmmo;
    public float laserBeamSpeed;
    private bool isRecharging = false;
    //Fire ball
    public GameObject fireBallPrefab;
    public float fireballCooldown;
    private float fireballCooldownTimer = 0;
    public int maxBounceCount;

    private CameraShake cameraShake;
    private EnemyEntranceEffects entranceEffects;

    public GameObject lightningImpactParticles;
    public Transform transformToInstantiateParticlesAt;
    public AudioSource projectileAudio;
    public AudioClip lightningThrash;

    private bool areGodsAllowedToAttack;


    public void Start()
    {
        
        emoteDuration = 0;
        emoteMaxTime = 1f;
        isEmoting = false;

        if (runtimeChoices.chosenGods[godNumber - 1] == null)
        {
            runtimeChoices.chosenGods[godNumber - 1] = availableGods[UnityEngine.Random.Range(0, 3)];
        }
        FindObjectOfType<AudioList>().SetGodSounds();

        cameraShake = FindObjectOfType<CameraShake>();
        entranceEffects = FindObjectOfType<EnemyEntranceEffects>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (runtimeChoices.chosenGods.Length >= godNumber && gamesettings.GetAmountOfPlayers() > godNumber)
        {

            godInfo = runtimeChoices.chosenGods[godNumber - 1];

            sprite = godInfo.topBarIcon;
            attackType = godInfo.attackType;
            spriteRenderer.sprite = sprite;
            tmproName.text = godInfo.godName;

            isEnabled = true;
            Debug.Log("Finished god-initializing");
        }
        else
        {
            spriteRenderer.sprite = null;
            isEnabled = false;
            gameObject.SetActive(false);
        }

        if (readyForFire == null)
        {
            return;
        }
        readyForFire.text = "Ready to Fire!";
    }

    public void AssignRandomGod()
    {
        runtimeChoices.chosenGods[godNumber - 1] = availableGods[UnityEngine.Random.Range(0, 3)];
        FindObjectOfType<AudioList>().SetGodSounds();
        godInfo = runtimeChoices.chosenGods[godNumber - 1];
        sprite = godInfo.topBarIcon;
        attackType = godInfo.attackType;
        spriteRenderer.sprite = sprite;
        tmproName.text = godInfo.godName;
    }

    public void ToggleCombatMode(bool active)
    {
        inCombatMode = active;
    }

    void Update()
    {
        #region Movement
        float direction = Input.GetAxis(horizontalAxis);
        if (Input.GetKey(horiRightKeyboardTestKey))
        {
            direction = 1;
        }
        if (Input.GetKey(horiLeftKeyboardTestKey))
        {
            direction = -1;
        }


        Debug.Log("Direction Input: " + direction);
        transform.position = transform.position + new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0);
        #endregion

        // inCombatMode is from legacy code - don't know if its to be used
        if ((Input.GetKeyDown(shoot) || Input.GetKeyDown(altShoot)) && canAttack && inCombatMode)
            #region Attack

            switch (attackType)
            {
                case GodInformation.AttackTypes.Lightning:
                    if (canAttack)
                        StartCoroutine(LightningStrikeAttack());
                    break;
                case GodInformation.AttackTypes.Fireball:
                    FireballAttack();
                    break;
                case GodInformation.AttackTypes.Laserbeam:
                    LaserBeamAttack();
                    Debug.Log("this many " + LaserProjectile.projectileCount);
                    break;
                default:
                    Debug.Log(gameObject.name + "'s attacktype hasn't been set - head to the inspector to do that");
                    break;
            }
        if (attackType == GodInformation.AttackTypes.Fireball)
            FireballUpdate();
        if (attackType == GodInformation.AttackTypes.Laserbeam)
            LaserUpdate();
        #endregion


        #region Legacy Attack (outcommented)
        /*
            if (Input.GetKeyDown(shoot) && OnCooldown == false && inCombatMode == true)
            {
                Shoot();
                timer = 0;
                readyForFire.text = "Cooling off!";
                OnCooldown = true;
            }
        if (OnCooldown)
        {
            timer += Time.deltaTime;
        }
        if (timer > cooldownTime)
        {
            OnCooldown = false;
            timer = 0;
            if (!inCombatMode)
            {
                return;
            }
            readyForFire.text = "Press ↓ to Fire!";

        }
        */

        if(Input.GetKeyDown(controllerEmoteKey)){
            Emote();
        }
        #endregion
        if (!isEmoting)
        {
            return;
        }

        emoteDuration += Time.deltaTime;
        if (emoteMaxTime <= emoteDuration)
        {
            isEmoting = false;
            emoteDuration = 0;
            emoteRenderer.sprite = null;
        }
    }

    public int GetGodNumber()
    {
        return godNumber;
    }

    #region Legacy shoot function (outcommented)
    /*
    public void Shoot()
    {
        Debug.Log("Lightning");
        GameObject instance = Instantiate(lightningPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.down * lightningSpeed, ForceMode2D.Impulse);
    }
    */
    #endregion

    public void Emote()
    {
        emoteRenderer.sprite = emotes[UnityEngine.Random.Range(0, emotes.Length)];
        isEmoting = true;
        emoteDuration = 0;
    }

    public void OnMonsterDied()
    {
        inCombatMode = false;
        readyForFire.text = "Waiting for combat!";
    }

    public void OnHeroCaged()
    {
        inCombatMode = false;
        readyForFire.text = "Waiting for combat!";
    }

    public void OnHeroReleasedFromCage()
    {
        inCombatMode = true;
        readyForFire.text = "Ready to fire!";
    }

    IEnumerator LightningStrikeAttack()
    {
        readyForFire.text = "Firing!";
            float normalMovespeed = moveSpeed;
        Debug.Log("Lightning");
        canAttack = false;
        GameObject LightningStrikeClone = Instantiate(lightningStrikePrefab, transform);
        LightningProjectile lightningScript = LightningStrikeClone.GetComponent<LightningProjectile>();
        Light2D telegraph = lightningScript.telegraph.GetComponent<Light2D>();
        GameObject Lightning = lightningScript.Lightning;
        lightningScript.godNumber = GetGodNumber();

        // telegraph attack
        moveSpeed = moveSpeed / 2;

        float timer = 0f;

        while (timer < chargeTime && chargeTime != 0)
        {
            spriteRenderer.sprite = godInfo.whenCharging;
            telegraph.intensity = Map(timer, 0, chargeTime, minIntensity, maxIntensity);
            // Debug.Log(timer + " intensity: " + telegraph.intensity);
            timer += Time.deltaTime;
            yield return null;
        }

        // actual attack
        telegraph.gameObject.SetActive(false);
        spriteRenderer.sprite = godInfo.whenShooting;
        Lightning.SetActive(true);
        entranceEffects.StartChromaticAberration(0.5f, 1f);
        cameraShake.StartShake(cameraShake.shakePropertyOnMinionEnter);
        GameObject go = Instantiate(lightningImpactParticles, transformToInstantiateParticlesAt.position, Quaternion.identity);
        Destroy(go, 4f);
        projectileAudio.clip = lightningThrash;
        projectileAudio.time = 0.2f;
        projectileAudio.Play();

        moveSpeed = 0;
        Light2D[] lights = transform.GetComponentsInChildren<Light2D>();
        Debug.Log("Lights: " + lights);
        foreach (Light2D light in lights)
        {
            StartCoroutine(RampDownLight(light, 0.5f));
        }
        readyForFire.text = "Cooling off!";
        yield return new WaitForSeconds(0.5f);
        Destroy(LightningStrikeClone);
        spriteRenderer.sprite = godInfo.topBarIcon;
        yield return new WaitForSeconds(4.5f);
        readyForFire.text = "Ready to fire!";
        canAttack = true;
        
        moveSpeed = normalMovespeed;
    }

    IEnumerator RampDownLight(Light2D impactLight, float duration)
    {
        float timer = duration;
        float originalIntensity = impactLight.intensity;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            impactLight.intensity = Map(timer, duration, 0, originalIntensity, 5);
            yield return new WaitForSeconds(0);
        }
        yield return null;
    }

    private void FireballAttack()
    {

        if (fireballCooldownTimer > 0)
            return;

        FindObjectOfType<AudioList>().godSources[GetGodNumber() - 1].Play();
        fireballCooldownTimer = fireballCooldown;
        GameObject fireballClone = Instantiate(fireBallPrefab, transform.position, Quaternion.identity, null);
        if (maxBounceCount > 0)
            fireballClone.GetComponentInChildren<FireballProjectile>().bounceLimit = maxBounceCount;
        fireballClone.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
        fireballClone.GetComponentInChildren<FireballProjectile>().godNumber = GetGodNumber();
    }
    private void LaserBeamAttack()
    {

        if (LaserProjectile.projectileCount >= laserAmmo)
            return;
        FindObjectOfType<AudioList>().godSources[GetGodNumber() - 1].Play();
        GameObject laserClone = Instantiate(laserBeamPrefab, transform.position, Quaternion.Euler(0, 0, 90), null);
        laserClone.GetComponent<Rigidbody2D>().velocity = Vector2.down * laserBeamSpeed * 2;
        laserClone.GetComponentInChildren<LaserProjectile>().godNumber = GetGodNumber();
    }

    // necessary to control cooldown and the likes
    private void LaserUpdate()
    {
        if (LaserProjectile.projectileCount > 0 && !isRecharging)
        {
            StartCooldown();
            return;
        }
        if (isRecharging)
        {
            laserCooldownTimer = UpdateCooldown(laserCooldownTimer);
            
            if (laserCooldownTimer > 0)
            {
                return;
            }
            Debug.Log("got one ammo back");
            LaserProjectile.projectileCount--;
            laserCooldownTimer = laserCooldown;
            if (LaserProjectile.projectileCount == 0)
            {
                isRecharging = false;
                readyForFire.text = "Cooling off!";
            }

        }
    }
    private void FireballUpdate()
    {
        readyForFire.text = "Ready to fire!";
        if (fireballCooldownTimer > 0)
            readyForFire.text = "Cooling off!";

        fireballCooldownTimer = UpdateCooldown(fireballCooldownTimer);
    }

    private float UpdateCooldown(float cooldown)
    {
        return Mathf.Max(cooldown - Time.deltaTime, 0f);
    }

    private void StartCooldown()
    {
        laserCooldownTimer = laserCooldown;
        isRecharging = true;
        readyForFire.text = "Cooling off!";
    }

    float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public void SetGodsAllowedToAttack(bool areGodsAllowed)
    {
        areGodsAllowedToAttack = areGodsAllowed;
    }
}