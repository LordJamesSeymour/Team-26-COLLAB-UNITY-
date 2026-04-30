using Group26.Player.Inputs;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Linq;

namespace Group26.Player.Movement
{
    public class StyleSystem : MonoBehaviour
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

        public bool ComboDecayBool = false;

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

        private string LastState = "default";
        public bool GrappleBoostState = false;
        private bool exists = false;

        private void Awake()
        {
            if (UI_Canvas == null)
            {
                UI_Canvas = UI_Refrence.GetComponent<Canvas>();
            }
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

            //playerController.TrickSystemEvent += TrickSystemMain;

        }

        public void DoATrick()
        {
            print(TotalScore);
        }

        private void FixedUpdate()
        {
            float SmoothScore = Mathf.SmoothDamp(DecayMeterSlider.value, DefaultPointMultiplier, ref CurrentVelocitySlider, DecaySliderSmoothness * Time.deltaTime);
            DecayMeterSlider.value = SmoothScore;

        }

        public void PointsCalculation(int Points, float Decay)
        {
            TotalScore += (int)(Points * Decay);
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
            /*
            if (UI_Text_Order == 5)
            {
                UI_Text_Order = 1;
            }
            else
            {
                UI_Texts[UI_Text_Order].text = "+ " + Name;
                UI_Text_Order += 1;
            }
            */

            for (int i = 1; i < UI_Texts.Length; i++)
            {
                //Debug.Log("Loop");
                //Debug.Log("Name Existss?" + UI_Texts[i].text.Contains(Name.ToString()));
                //Debug.Log("Name is :" + Name.ToString());
                //Debug.Log(UI_Texts[i].text);

                if (UI_Texts[i].text.Contains(Name.ToString()))
                {
                    //Debug.Log("Exists :" + Name.ToString());
                    exists = true;
                }
                else
                {
                    exists = false;
                }

                if (exists == true)
                {
                    //Debug.Log("Exists == True");
                    if (UI_Texts[i].text.ToString().Any(char.IsDigit))
                    {
                        //Debug.Log("Has A digit");
                        int ComboNumbers = int.Parse(Regex.Match(UI_Texts[i].text, @"\d+").Value) + 1;
                        //Debug.Log("DIgit : " + ComboNumbers);
                        UI_Texts[i].text = "+ " + Name + " " + (ComboNumbers) + "x ";
                        break;
                    }
                    else
                    {
                        //Debug.Log("No digit");
                        UI_Texts[i].text = "+ " + Name + " " + 2 + "x ";
                        break;
                    }
                }
                else if (!exists && !UI_Texts[i].text.EndsWith("x ") && i < 5)
                {
                    //Debug.Log("Empty socket");
                    UI_Texts[i].text = "+ " + Name;
                    break;
                }
            }
            /*
            for (int i = 1; i < UI_Texts.Length; i++)
            {
                if (i == 5)
                {
                    Debug.Log("Break");
                    break;
                }
                else if (UI_Texts[i].text.Contains(Name.ToString()) )
                {
                    //Debug.Log(int.TryParse(UI_Texts[i].text.Substring(Name.Length + 1), out UI_Text_Order));
                    //UI_Texts[i].text = UI_Texts[i].text + " " + 1 + "x ";
                    if (UI_Texts[i].text.ToString().Any(char.IsDigit))
                    {
                        int ComboNumbers = int.Parse(Regex.Match(UI_Texts[i].text, @"\d+").Value);
                        UI_Texts[i].text = UI_Texts[i].text + " " + (ComboNumbers + 1) + "x ";
                    }
                    else
                    {
                        UI_Texts[i].text = UI_Texts[i].text + " " + 2 + "x ";
                    }
                    

                }


            }
            */

            if(GrappleBoostState)
            {
                GrappleBoostState = false;
            }

            // for some reason grapple triggers dash 3 times
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

        public void AddStyleCombo(int Points, string State, string UIText)
        {
            Buffer += 1;
            

            if (Buffer == 12)
            {
                //WallRunningPointsEnabled = false;
                DecayCalculation(State);
                PointsCalculation(Points, ComboDecay);
                UITextOrder(UIText);
            }
            else if (Buffer > 12)
            {
                Buffer = 0;
            }

        }

        /*
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
    }*/


    }
}

