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
        [SerializeField] public float DecaySliderSmoothness = 100;
        [SerializeField] public float ConstnatDecaySliderSmoothness = 100;
        private Canvas UI_Canvas;
        private TMP_Text[] UI_Texts;
        private int UI_Text_Order = 1;
        private Slider DecayMeterSlider;
        private float CurrentVelocitySlider = 0;
        //fix later vvv
        //private int UI_TextComboInt = 1;

        [Header("Combo")]
        [SerializeField] public int TotalScore;
        [SerializeField] public float DefaultPointMultiplier = 1;
        [SerializeField] public float ComboDecay = 0.5f;
        [SerializeField] public float ConstantPointsDecay = 0.001f;

        public bool ComboDecayBool= false;

        [Header("Points for trick")]
        [SerializeField] private int PointsForDash = 5;
        [SerializeField] private int PointsForWallRun = 10;
        [SerializeField] private int PointsForGrapple = 5;
        [SerializeField] private int PointsForSlide = 10;

        [Header("Timers")]
        [SerializeField] public float MaxTimer = 0;
        [SerializeField] private float CurrentTimer = 0;

        private bool WallRunningPointsEnabled = true;
        private bool SwingingPointsEnabled = true;

        [Header("States")]
        [SerializeField] private string LastState = "default";

        [Header("Debug")]
        [SerializeField] private int m_maxWhileIters = 1000;
        //[SerializeField] private int DebugScore = 0;

        private void Awake()
        {
            //if (UI_Canvas == null)
            //{
            //    UI_Canvas = UI_Refrence.GetComponent<Canvas>();
            //}
            UI_Texts = UI_Canvas.gameObject.GetComponentsInChildren<TMP_Text>();
            DecayMeterSlider = UI_Canvas.GetComponentInChildren<Slider>();

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

            playerController.TrickSystemEvent += TrickSystemMain;

        }

        public void DoATrick() 
        {
            print(TotalScore);
        }

        private void FixedUpdate()
        {
            float SmoothScore = Mathf.SmoothDamp(DecayMeterSlider.value, DefaultPointMultiplier, ref CurrentVelocitySlider, DecaySliderSmoothness * Time.deltaTime);
            DecayMeterSlider.value = SmoothScore;

            /*
            if (ComboDecayBool == false)
            {
                float ConstantDecay = DecayMeterSlider.value - ConstantPointsDecay;
                float ScoreMultDecay = Mathf.SmoothDamp(DecayMeterSlider.value, ConstantDecay, ref CurrentVelocitySlider, ConstnatDecaySliderSmoothness * Time.deltaTime);

                DecayMeterSlider.value = ConstantDecay;
            }
            else
            {
                

                float SmoothScore = Mathf.SmoothDamp(DecayMeterSlider.value, DefaultPointMultiplier, ref CurrentVelocitySlider, DecaySliderSmoothness * Time.deltaTime);
                DecayMeterSlider.value = SmoothScore;
                ComboDecayBool = false;
            }
            */
        }

        public void PointsCalculation(int Points, float Decay)
        {
            TotalScore += (int)(Points * Decay);
            //print(TotalScore);
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
                DefaultPointMultiplier = Mathf.Clamp(DefaultPointMultiplier, 0, 3);

                UI_Texts[6].text = "Mult: x" + DefaultPointMultiplier;
    
            }

            if (State != LastState)
            {
                DefaultPointMultiplier += ComboDecay;
                DefaultPointMultiplier = Mathf.Clamp(DefaultPointMultiplier, 0, 3);

                UI_Texts[6].text = "Mult: x" + DefaultPointMultiplier;

            }

            LastState = State;
        }

        
        public void UITextOrder(string Name)
        {
            UI_Texts[5].text = "Score: " + TotalScore.ToString();

            if (UI_Text_Order >= 5)
            {
                UI_Text_Order = 1;
            }
            else
            {
                UI_Texts[UI_Text_Order].text = "+ " + Name;
                UI_Text_Order += 1;
            }
            /*
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
            */
        }

        // Update is called once per frame
        void Update()
        {

            

            if (CurrentTimer < MaxTimer)
            {
                CurrentTimer += Time.deltaTime;
            }

            if (CurrentTimer >= MaxTimer)
            {
                WallRunningPointsEnabled = true;
                SwingingPointsEnabled = true;
                CurrentTimer = 0;

            }

        }

        private void TrickSystemMain()
        {
            Buffer += 1;

            //makes sure that you don't get 12x amount of point a second (if there is a better way to make this please tell me)
            if (Buffer == 12)
            {

                if (playerController.state == PlayerController.MovementState.dashing)
                {
                   // Debug.Log("Dash");

                    PointsCalculation(PointsForDash, DefaultPointMultiplier);
                    DecayCalculation(PlayerController.MovementState.dashing.ToString());
                    UITextOrder("Dash");
                }

                if (playerController.state == PlayerController.MovementState.wallRunning)
                {
                    //Debug.Log("Wall Running");
                    if (WallRunningPointsEnabled)
                    {
                        WallRunningPointsEnabled = false;
                        DecayCalculation(PlayerController.MovementState.wallRunning.ToString());
                        PointsCalculation(PointsForWallRun, DefaultPointMultiplier);
                        UITextOrder("Wall Run");
                    }

                }

                if (playerController.state == PlayerController.MovementState.swinging)
                {
                    //Debug.Log("Swinging");
                    if (SwingingPointsEnabled)
                    {
                        SwingingPointsEnabled = false;
                        PointsCalculation(PointsForGrapple, DefaultPointMultiplier);
                        DecayCalculation(PlayerController.MovementState.swinging.ToString());
                        UITextOrder("Grapple");
                    }
                }

                if (playerController.state == PlayerController.MovementState.sliding)
                {
                    //Debug.Log("Sliding");
                    PointsCalculation(PointsForSlide, DefaultPointMultiplier);
                    DecayCalculation(PlayerController.MovementState.sliding.ToString());
                    UITextOrder("Slide");
                }

                ComboDecayBool = true;
            }
            else if (Buffer > 12)
            {
                Buffer = 0;
            }

        }
    }

}
