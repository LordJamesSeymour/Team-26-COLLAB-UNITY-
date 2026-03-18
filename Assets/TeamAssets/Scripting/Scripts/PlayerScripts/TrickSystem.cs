using Group26.Player.Inputs;
using UnityEngine;

namespace Group26.Player.Movement
{
    public class TrickSystem : MonoBehaviour
    {
        private InputManager InputManager;
        private PlayerController playerController;
        private string[] StateValidationCheck;

        private int Buffer;
        public int TotalScore;

        public int ActionScoreLimit = 3;
        public int CurrentActionScore;


        public string LastState;

        //public enum ActionState
        //{
        //    sliding,
        //    swinging,
        //    wallRunning,
        //    dashing
        //}

        private void Awake()
        {
            if (InputManager == null)
            {

                InputManager = GetComponent<InputManager>();
            }

            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

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

            //print("Current State: " + playerController.state);
            //print(CurrentActionScore + "    Current");
            //print(ActionScoreLimit + "   Limit");
            //print(StateValidationCheck[1].ToString());
            print(TotalScore);

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

            //int EnumLength = Enum.GetValues(typeof(ActionState)).Length;
            //StateValidationCheck = new string[EnumLength];

            //for (int i = 0; i < EnumLength; i++)
            //{
            //    ActionState ActionState = (ActionState)i;
            //    string StateName = ActionState.ToString();

            //    StateValidationCheck[i] = StateName;

            //}

        }

        private void FixedUpdate()
        {

            Buffer += 1;

            //makes sure that you don't get 12x amount of point a second (if there is a better way to make this please tell me)
            if (Buffer == 12)
            {

                if (playerController.state == PlayerController.MovementState.dashing)
                {
                    Debug.Log("Dash");
                    TotalScore += 5;

                }

                if (playerController.state == PlayerController.MovementState.wallRunning)
                {
                    Debug.Log("Wall Running");
                    TotalScore += 7;
                }

                if (playerController.state == PlayerController.MovementState.swinging)
                {
                    Debug.Log("Swinging");
                    TotalScore += 7;
                }

                if (playerController.state == PlayerController.MovementState.sliding)
                {
                    Debug.Log("Sliding");
                    TotalScore += 5;
                }

                //for (int i = 0; i < StateValidationCheck.Length; i++) //cheks if current state matches any of the ones listed as "ActionState"
                //{
                //    if (playerController.state.ToString() == StateValidationCheck[i])
                //    {

                //        /*
                //        if (Controller.state.ToString() == StateValidationCheck[1])
                //        {
                //            print("Swing");

                //            if (CurrentGrappleScore < GrappleScoreLimit)
                //            {
                //                CurrentGrappleScore += 1;
                //                print(CurrentGrappleScore);
                //                TotalScore += 10;

                //            }



                //        }
                //        */

                //        if (CurrentActionScore < ActionScoreLimit)
                //        {
                //            CurrentActionScore += 1;
                //            //print(CurrentActionScore);
                //            TotalScore += (10 / (1 + CurrentActionScore));

                //        }

                //        // +++ Need to add point decay upon doing same trick multiple times 

                //        LastState = playerController.state.ToString();

                //        print("Current Score: " + TotalScore);

                //        Buffer = 0;
                //        break;
                //    }

                //    //Limits Amount of points gaied from hangingfrom grapple
                //    if (CurrentActionScore >= ActionScoreLimit && playerController.state.ToString() != LastState)
                //    {
                //        //print("Limit");
                //        CurrentActionScore = 0;
                //    }

                //}
            }
            else if (Buffer > 12)
            {
                Buffer = 0;
            }


        }

        public void PointsCalculation(int Points, float Decay)
        {
            TotalScore += (int)(Points * Decay);
        }


        // Update is called once per frame
        void Update()
        {





        }
    }
}
