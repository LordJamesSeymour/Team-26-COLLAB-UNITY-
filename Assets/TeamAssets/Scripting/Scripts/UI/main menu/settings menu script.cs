using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class settingsmenuscript : menuscreenscript
{
    [SerializeField] Toggle m_checkpointToggle;

    private Coroutine m_toggle;
    private datamanager m_manager;
    private static bool m_run;

    protected override void Awake()
    {
        base.Awake();

        m_manager = new datamanager(3);

        if (m_run == false)
        {
            m_run = true;
        }
        else
        {
            m_manager.LoadGameData();
        }

        Debug.Log(m_manager.GetGameData().settings.checkpointsEnabled);
        m_checkpointToggle.isOn = m_manager.GetGameData().settings.checkpointsEnabled;
    }

    private IEnumerator ToggleSettingsMenuOff()
    {
        m_buttonScript.m_settingsPanel.SetActive(false);
        m_buttonScript.m_mainMenuPanel.SetActive(true);
        m_onExitButton = false;
        m_enabled = false;
        yield return new WaitUntil(() => m_buttonScript.m_settingsPanel.activeSelf == false && m_buttonScript.m_mainMenuPanel.activeSelf == true);
        m_buttonScript.m_mainMenuPanelEnabled = true;
        m_toggle = null;
        StopCoroutine(ToggleSettingsMenuOff());
    }

    public void RunToggleSettingsOff()
    {
        if(m_toggle == null)
            m_toggle = StartCoroutine(ToggleSettingsMenuOff());
    }

    public void ToggleCheckpointsEnabled()
    {
        /*Checkpoint.m_checkpointsEnabled = !Checkpoint.m_checkpointsEnabled*/;
        m_manager.LoadGameData();
        m_manager.SetCheckpointsEnabled(m_checkpointToggle.isOn);
        m_manager.SaveGameData();
        Debug.Log(m_manager.GetGameData().settings.checkpointsEnabled);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
