using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class settingsmenuscript : menuscreenscript
{
    [SerializeField] Toggle m_checkpointToggle;

    private Coroutine m_toggle;

    protected override void Awake()
    {
        base.Awake();
        m_checkpointToggle.isOn = Checkpoint.m_checkpointsEnabled;
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
        Checkpoint.m_checkpointsEnabled = !Checkpoint.m_checkpointsEnabled;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
