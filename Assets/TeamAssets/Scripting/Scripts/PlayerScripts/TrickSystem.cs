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
        InputManager.OnTrickPressed += DoATrick;
    }

    private void OnDisable()
    {
        InputManager.OnTrickPressed -= DoATrick;
    }

    public void DoATrick()
    {
        /*
        print("Do a Flip!");
        Object component = transform.GetChild(0); //Finds PlayerBody;
        Object orb = GetComponent<Object>();

        PlayerController controller = orb.GetComponent<PlayerController>();
        //bool b = PlayerController..GetComponent<PlayerController>().
        
        if (Playe == null)
        {
            print("PlayerController is null");
        }
        
        if (controller == null)
        {
            print("Controller is null");
        }


        if (component != null)
        {

            Renderer Renderer = component.GetComponent<Renderer>();
            Color color = Renderer.material.color;
            Renderer.material.SetColor("_BaseColor", Color.white);



        }
        */

        print("Do a Flip!");
        

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

        //print(Buffer);
        if (Buffer == 12)
        {
            for (int i = 0; i < StateValidationCheck.Length; i++)
            {
                if (Controller.state.ToString() == StateValidationCheck[i])
                {

                    //fix ininite score bug with grapple 
                    //fine tune the score system
                    print("combo");

                    TotalScore += 5;

                    print("Current Score: " + TotalScore);

                    Buffer = 0;
                    break;
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
