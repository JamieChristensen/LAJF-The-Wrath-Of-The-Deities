﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //TODO: Change these objects to corresponding classes as they are implemented.
    [SerializeField]
    private P1Controller player1;
    [SerializeField]
    private Object God1, God2, God3; 
    //Should just be an array of gods, loop through the input axes using index
    //when for-looping through them.
    //Initialize "gods" when the amount of players has been selected.

    private float player1Hori;
    private float player1Jump;
    private float player1Attack;



    //TODO: Figure out which inputs to use
    //Probably need to implement border-style selection of portraits for each god and just have left-right inputs to decide which one to go to
    //based on current one.

    //Player1 will have WASD/arrowbuttons for control, and spacebar + some button, for shooting (and a proper control scheme for controllers)
    //Players2,3,4 will need some alternate controls on keyboard too - but otherwise controller usage too.



    private void Update()
    {
        GetInputs();
        SendInputs();
    }

    private void GetInputs()
    {
        if (player1 != null)
        {
            player1Hori = Input.GetAxis("P1Horizontal");
            player1Jump = Input.GetAxis("P1Jump");
            player1Attack = Input.GetAxis("P1Shoot");
        }

        if (God1 != null)
        {

        }

        if (God2 != null)
        {

        }

        if (God3 != null)
        {

        }

    }

    private void SendInputs()
    {
        if (player1 != null)
        {
            player1.ReceiveInput(P1Controller.Player1Input.Attack, player1Attack);
            player1.ReceiveInput(P1Controller.Player1Input.Jump, player1Jump);
            player1.ReceiveInput(P1Controller.Player1Input.Horizontal, player1Hori);
        }

        //Implement gods
    }
}