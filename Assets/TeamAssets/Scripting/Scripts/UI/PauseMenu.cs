using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] Button m_resume;
    [SerializeField] Button m_restart;
    [SerializeField] Button m_controls;
    [SerializeField] Button m_settings;
    [SerializeField] Button m_return;

    private void Awake()
    {
        m_resume.onClick.AddListener(Resume);
        m_restart.onClick.AddListener(Restart);
        m_controls.onClick.AddListener(Controls);
        m_settings.onClick.AddListener(Settings);
        m_return.onClick.AddListener(Return);
    }

    // listen on player for esc, if clicked call gameObject.SetActive(!gameObject.activeSelf); // switch menu state

    public void Resume() // close pause menu (will need to stop and start gameplay)
    {
        Debug.Log("Resume");
        gameObject.SetActive(false);
    }
    private void Restart() // reload this scene
    {
        Debug.Log("Restart");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void Controls() // open controls image
    {
        Debug.Log("Controls");
    }
    private void Settings()         // open settings sub menu
    {
        // only setting so far is sense on mouse and controller
        Debug.Log("Settings");
    }
    private void Return() //load map scene
    {
        Debug.Log("Return");
    }
}
