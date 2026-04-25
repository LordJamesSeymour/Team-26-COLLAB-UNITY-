using Group26.Player.Camera;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TEST_level_button_script : MonoBehaviour
{
    private datamanager m_manager;

    public int m_levelNum;  //this is the level's number - 1 e.g. if this is level 1 then this var is 0

    [SerializeField] private TextMeshProUGUI m_levelText;
    //[SerializeField] private CameraModeManager m_cameraScript;

    private void Awake()
    {
        m_manager = new datamanager(6);
        try
        {
            m_manager.LoadGameData();
            Debug.Log("loaded");
        }
        catch(Exception e)
        {
            Debug.Log(e.Message + "not settings");
        }
    }

    public void CompleteLevel(int scene)
    {
        m_manager.SetCompleted(m_levelNum, true);
        m_manager.SaveGameData();
        //m_manager.LoadGameData();
        //Debug.Log(m_manager.GetGameData().levels[0].completed);
        SceneManager.LoadScene(scene);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //m_manager.LoadGameData();
        Debug.Log(m_manager.GetGameData().levels[m_levelNum].levelNum);

        m_levelText.text = "Level: " + m_manager.GetGameData().levels[m_levelNum].levelNum;
        //Debug.Log(m_cameraScript.thirdPersonLookSensitivity);
    }
}
