﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "P1Stats", menuName = "Stat/P1Stats")]
public class P1Stats : ScriptableObject
{
    [Header("General")]
    public string name; // name of the character
    public string description; // description of character
    public string trait; // the traits of choosing this character

    [Header("Movement")]
    public float moveSpeed; //the speed the player is able to move with
    public float jumpForce; //how high the player jump
    
    [Header("Attack")]
    public float baseAttackDamage; // the starting attack damage of the character
    public float attackRate; // minimum time between attacks
    public bool rangedAttacks; // can perform ranged attacks
    public bool meleeAttacks; // can perform melee attacks

    [Header("Defense")]
    public int maxHitPoints; // maximum amount of HP
    public int startingHitPoints; // the amount of HP in the beginining of the run
    public float baseDamageReduction; // the starting damage reduction of the character

}