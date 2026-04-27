using Group26.Player.Inputs;
using Group26.Player.Movement;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] public GameObject PlayerReference;
    [SerializeField] public TMP_Text TimerReference;
    private InputManager InputManager;


    [Header("Trick Rank Reqirements")]
    [SerializeField] private int TrickRankF = 10;
    [SerializeField] private int TrickRankE = 20;
    [SerializeField] private int TrickRankD = 30;
    [SerializeField] private int TrickRankC = 40;
    [SerializeField] private int TrickRankB = 50;
    [SerializeField] private int TrickRankA = 60;
    [SerializeField] private int TrickRankS = 70;
    private int[] TrickRanksArray;


    [Header("Time Rank Reqirements")]
    [SerializeField] private int TimeRankF = 420;
    [SerializeField] private int TimeRankE = 360;
    [SerializeField] private int TimeRankD = 300;
    [SerializeField] private int TimeRankC = 240;
    [SerializeField] private int TimeRankB = 180;
    [SerializeField] private int TimeRankA = 120;
    [SerializeField] private int TimeRankS = 60;
    private int[] TimeRanksArray;

    [Header("Collectables")]
    [SerializeField] public int CollecablesCollected;
    [SerializeField] public int CollectablePoints;
    private GameObject[] AllCollectedArray;

    [Header("Collectables Rank Reqirements")]
    [SerializeField] private int CollectablesAmountF = 0;
    [SerializeField] private int CollectablesAmountE = 1;
    [SerializeField] private int CollectablesAmountD = 2;
    [SerializeField] private int CollectablesAmountC = 3;
    [SerializeField] private int CollectablesAmountB = 4;
    [SerializeField] private int CollectablesAmountA = 5;
    [SerializeField] private int CollectablesAmountS = 6;
    private int[] CollectableRankArray;

    private string[] RankNamesArray;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AllCollectedArray = GameObject.FindGameObjectsWithTag("Collectable");
        Debug.Log(AllCollectedArray.Length);
    }

    private void Awake()
    {
        if (InputManager == null)
        {
            InputManager = PlayerReference.gameObject.GetComponent<InputManager>();
        }

        TrickRanksArray = new int[7] {TrickRankS, TrickRankA, TrickRankB, TrickRankC, TrickRankD, TrickRankE, TrickRankF };
        TimeRanksArray = new int[7] { TimeRankS, TimeRankA, TimeRankB, TimeRankC, TimeRankD, TimeRankE, TimeRankF };
        CollectableRankArray = new int[7] { CollectablesAmountS, CollectablesAmountA, CollectablesAmountB, CollectablesAmountC, CollectablesAmountD, CollectablesAmountE, CollectablesAmountF };
        RankNamesArray = new string[7] { "Rank S", "Rank A", "Rank B", "Rank C", "Rank D", "Rank E", "Rank F" };
    }

    private void OnEnable()
    {
        InputManager.OnRankDisplayPressed += FinalScoreResultCalculation;
    }

    private void OnDisable()
    {
        InputManager.OnRankDisplayPressed -= FinalScoreResultCalculation;
    }

    private void FinalScoreResultCalculation()
    {
        int TrickScore = PlayerReference.GetComponent<TrickSystem>().TotalScore;
        int TimeScore = TimerReference.GetComponent<Timer>().m_totalTimeSecs;

        for (int i = 0; i < TrickRanksArray.Length; i++)
        {
            if (TrickScore >= TrickRanksArray[i])
            {
                Debug.Log("Rank for Tricks:      " + RankNamesArray[i]);
                break;
            }

        }

        for (int i = 0; i < TimeRanksArray.Length; i++)
        {
            if (TimeScore <= TimeRanksArray[i])
            {
                Debug.Log("Rank for Time:      " + RankNamesArray[i]);
                break;
            }

        }

        for (int i = 0; i < CollectableRankArray.Length; i++)
        {
            if (CollecablesCollected >= CollectableRankArray[i])
            {
                Debug.Log("Rank for Collectables:      " + RankNamesArray[i]);
                break;
            }

        }

    }

}

[System.Serializable]
public class TrickRanks
{
    public int Rank;
}
