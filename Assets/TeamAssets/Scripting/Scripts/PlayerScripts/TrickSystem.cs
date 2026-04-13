using Group26.Player.Inputs;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

namespace Group26.Player.Movement
{
    public class TrickSystem : MonoBehaviour
    {
        private InputManager InputManager;
        private PlayerController playerController;
        private string[] StateValidationCheck;

        private int Buffer;

        [Header("UI")]
        [SerializeField] public GameObject UI_Refrence;
        private Canvas UI_Canvas;
        private TMP_Text[] UI_Texts;
        private int UI_Text_Order = 1;
        private int UI_TextComboInt = 1;

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
            if (UI_Canvas == null)
            {
                UI_Canvas = UI_Refrence.GetComponent<Canvas>();
                //Debug.Log("MISSING TRICK SYSTEM UI");
            }
            UI_Texts = UI_Canvas.gameObject.GetComponentsInChildren<TMP_Text>();
            
            for (int i = 1; i < UI_Texts.Length; i++)
            {
                UI_Texts[i].text = " ";
            }

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

            print(TotalScore);

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

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
                    UITextOrder("Dash");
                }

                if (playerController.state == PlayerController.MovementState.wallRunning)
                {
                    Debug.Log("Wall Running");
                    if (WallRunningPointsEnabled)
                    {
                        WallRunningPointsEnabled = false;
                        DecayCalculation(PlayerController.MovementState.wallRunning.ToString());
                        PointsCalculation(7, DefaultPointMultiplier);
                        UITextOrder("Wall Run");
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
                        UITextOrder("Grapple");
                    }
                }

                if (playerController.state == PlayerController.MovementState.sliding)
                {
                    Debug.Log("Sliding");
                    PointsCalculation(5, DefaultPointMultiplier);
                    DecayCalculation(PlayerController.MovementState.sliding.ToString());
                    UITextOrder("Slide");
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

        public void UITextOrder(string Name)
        {

            if (UI_Text_Order >= 5)
            {
                UI_Text_Order = 1;
            }
            else
            {
                UI_Texts[UI_Text_Order].text = "+ " + Name;
                UI_Text_Order += 1;
            }

            for (int i = 1; i < UI_Texts.Length; i++)
            {
                if (("+ " + Name) == UI_Texts[i].text)
                {
                    //UI_Texts[i].text += " x"+ 2;
                }
                else
                {
                    
                }
            }

            
            
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
