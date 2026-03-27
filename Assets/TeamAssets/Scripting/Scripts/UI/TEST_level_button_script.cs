using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TEST_level_button_script : MonoBehaviour
{
    private datamanager m_manager;

    [SerializeField] private TextMeshProUGUI m_levelText;

    private void Awake()
    {
        m_manager = new datamanager(3);
    }

    public void CompleteLevel()
    {
        m_manager.SetCompleted(true);
        m_manager.SaveData();
        SceneManager.LoadScene(2);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_manager.LoadGameData();
        m_levelText.text = "Level: " + m_manager.GetGameData().levels[0].levelNum;
    }
}
