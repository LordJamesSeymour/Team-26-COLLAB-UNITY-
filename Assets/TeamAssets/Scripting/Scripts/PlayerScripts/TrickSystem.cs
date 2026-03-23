using Group26.Player.Inputs;
using System.Collections;
using UnityEngine;

namespace Group26.Player.Movement
{
    public class TrickSystem : MonoBehaviour
    {
        private InputManager InputManager;
        private PlayerController playerController;
        private string[] StateValidationCheck;

        private int Buffer;
        

        [Header("Combo")]
        [SerializeField] public int TotalScore;
        [SerializeField] public float DefaultPointMultiplier = 1;
        [SerializeField] public float ComboDecay = 0.5f;

        [Header("Timers")]
        [SerializeField] public float MaxTimer = 0;
        [SerializeField] private float CurrentTimer = 0;
        
        private bool WallRunningPointsEnabled = true;
        private bool SwingingPointsEnabled = true;

        [Header("States")]
        [SerializeField] private string LastState = "default";

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

                    PointsCalculation(5, DefaultPointMultiplier);
                    DecayCalculation(PlayerController.MovementState.dashing.ToString());
                }

                if (playerController.state == PlayerController.MovementState.wallRunning)
                {
                    Debug.Log("Wall Running");
                    if (WallRunningPointsEnabled)
                    {
                        WallRunningPointsEnabled = false;
                        DecayCalculation(PlayerController.MovementState.wallRunning.ToString());
                        PointsCalculation(7, DefaultPointMultiplier);
                    }
                    
                }

                if (playerController.state == PlayerController.MovementState.swinging)
                {
                    Debug.Log("Swinging");
                    if (SwingingPointsEnabled)
                    {
                        SwingingPointsEnabled = false;
                        PointsCalculation(7, DefaultPointMultiplier);
                        DecayCalculation(PlayerController.MovementState.swinging.ToString());
                    }
                }

                if (playerController.state == PlayerController.MovementState.sliding)
                {
                    Debug.Log("Sliding");
                    PointsCalculation(5, DefaultPointMultiplier);
                    DecayCalculation(PlayerController.MovementState.sliding.ToString());
                }

                
            }
            else if (Buffer > 12)
            {
                Buffer = 0;
            }


        }

        public void PointsCalculation(int Points, float Decay)
        {
            TotalScore += (int)(Points * Decay);
            print(TotalScore);
        }

        public void DecayCalculation(string State)
        {

            if (State == null)
            {
                Debug.Log("Decay Calculation state is NULL");
                return;
            }

            if (State == LastState)
            {
                DefaultPointMultiplier -= ComboDecay;
                DefaultPointMultiplier = Mathf.Clamp(DefaultPointMultiplier, 0, 1);
            }

            if (State != LastState)
            {
                DefaultPointMultiplier += ComboDecay;
                DefaultPointMultiplier = Mathf.Clamp(DefaultPointMultiplier, 0, 3);
            }

            LastState = State;
        }


        // Update is called once per frame
        void Update()
        {
            if (CurrentTimer < MaxTimer)
            {
                //print("false");
                CurrentTimer += Time.deltaTime;
                

            }

            if (CurrentTimer >= MaxTimer)
            {
                //print("true");
                WallRunningPointsEnabled = true;
                SwingingPointsEnabled = true;
                CurrentTimer = 0;

            }




        }
    }
}
