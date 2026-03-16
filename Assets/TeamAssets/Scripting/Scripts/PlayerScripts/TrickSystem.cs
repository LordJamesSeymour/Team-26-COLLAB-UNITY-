using Group26.Player.Inputs;
using Group26.Player.Movement;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static Group26.Player.Movement.PlayerController;

public class TrickSystem : MonoBehaviour
{
    private InputManager InputManager;
    private PlayerController Controller;
    private string[] StateValidationCheck;

    private int Buffer;
    public int TotalScore;

    public int GrappleScoreLimit = 3;
    public int CurrentGrappleScore;

    public string LastState;

    public enum ActionState
    {
        sliding,
        swinging,
        wallRunning,
        dashing
    }

    private void Awake()
    {
        if (InputManager == null) 
        {

            InputManager = GetComponent<InputManager>();
        }

        if (Controller == null)
        {
            Controller = GetComponent<PlayerController>();
            
        }

        /*
        StateValidationCheck[0] = MovementState.wallRunning.ToString();
        StateValidationCheck[1] = MovementState.wallRunning.ToString();
        StateValidationCheck[2] = MovementState.wallRunning.ToString();
        StateValidationCheck[3] = MovementState.wallRunning.ToString();
        */
    }

    private void OnEnable()
    {
        //print("2");
        InputManager.OnTrickPressed += DoATrick;
    }

    private void OnDisable()
    {
        //print("-2");
        InputManager.OnTrickPressed -= DoATrick;
    }

    public void DoATrick() //Currently bunch of debugs
    {
        //print("Do a Flip!");

        print("Current State: " + Controller.state);

        print(CurrentGrappleScore + "    Current");
        print(GrappleScoreLimit + "   Limit");

        print(StateValidationCheck[1].ToString());


    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int EnumLength = Enum.GetValues(typeof(ActionState)).Length;
        StateValidationCheck = new string[EnumLength];

        //print(EnumLength);

        for (int i = 0; i < EnumLength; i++)
        {
            ActionState ActionState = (ActionState)i;
            string StateName = ActionState.ToString();

            //print(StateName);

            StateValidationCheck[i] = StateName;

            //print(i);
            //Debug.Log(StateValidationCheck[i]);
        }

    }

    private void FixedUpdate()
    {
        
        Buffer += 1;


        //makes sure that you don't get 12x amount of point a second (if there is a better way to make this please tell me)
        if (Buffer == 12)
        {
            for (int i = 0; i < StateValidationCheck.Length; i++) //cheks if current state matches any of the ones listed as "ActionState"
            {
                if (Controller.state.ToString() == StateValidationCheck[i])
                {
                    /*
                    if (Controller.state.ToString() == StateValidationCheck[1])
                    {
                        print("Swing");

                        if (CurrentGrappleScore < GrappleScoreLimit)
                        {
                            CurrentGrappleScore += 1;
                            print(CurrentGrappleScore);
                            TotalScore += 10;
                            
                        }

                        
                        
                    }
                    */

                    if (CurrentGrappleScore < GrappleScoreLimit)
                    {
                        CurrentGrappleScore += 1;
                        print(CurrentGrappleScore);
                        TotalScore += 10;

                    }

                    // Need to add pint decay upon doing same trick multiple times 

                    LastState = Controller.state.ToString();

                    print("Current Score: " + TotalScore);

                    Buffer = 0;
                    break;
                }

                //Limits Amount of points gaied from hangingfrom grapple
                if (CurrentGrappleScore >= GrappleScoreLimit && Controller.state.ToString() != LastState)
                {
                    print("Limit");
                    CurrentGrappleScore = 0;
                }

            }
        }
        else if (Buffer > 12) 
        {
            Buffer = 0;
        }


    }


    // Update is called once per frame
    void Update()
    {

        



    }
}
