using System;
using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Death m_playerDeathScript;
    [SerializeField] GameObject m_floppyDisc;

    [HideInInspector] public bool m_used;
    public static bool m_checkpointsEnabled;    //this is static as if one checkpoint is disabled, all of them are
                                                //this variable is the one set in the settings menu when the player decides if
                                                //checkpoints are enabled or not
    private datamanager m_manager;
    private float m_floppyDiscStartPosY;
    private float m_maxPoint;
    private float m_minPoint;

    private void Awake()
    {
        //m_checkpointsEnabled = true;    //REMOVE THIS LINE WHEN SETTINGS MENU IS MADE
        m_manager = new datamanager(6);
        try
        {
            m_manager.LoadGameData();
        }
        catch (Exception e)
        {
            Debug.Log("Game data has not been loaded from main menu");
            //Debug.LogError(e.Message);
        }

        m_checkpointsEnabled = m_manager.GetGameData().settings.checkpointsEnabled;
        Debug.Log("checkpoints: " + m_manager.GetGameData().settings.checkpointsEnabled);
        m_floppyDiscStartPosY = m_floppyDisc.transform.position.y;
        m_maxPoint = m_floppyDiscStartPosY + 2.0f;
        m_minPoint = m_maxPoint - 0.7f;

        m_used = false;
        if(!m_checkpointsEnabled) gameObject.SetActive(false);
    }

    private IEnumerator MoveFloppyDisc()
    {
        bool movingUp = true;

        while (true)
        {
            while (/*m_floppyDisc.transform.position.y < m_floppyDicsStartPosY + 2.0f && */movingUp)
            {
                if (m_floppyDisc.transform.position.y >= m_maxPoint)
                    movingUp = false;
                m_floppyDisc.transform.position += new Vector3(0f, 0.7f * Time.deltaTime, 0f);
                yield return new WaitForEndOfFrame();
            }

            while (!movingUp)
            {
                if (m_floppyDisc.transform.position.y <= m_minPoint)
                    movingUp = true;

                m_floppyDisc.transform.position -= new Vector3(0f, 0.7f * Time.deltaTime, 0f);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && m_used == false)
        {
            m_playerDeathScript.m_respawnPoint = transform.position;
            m_playerDeathScript.m_respawnPoint.y = transform.position.y + 0.841f;
            m_playerDeathScript.m_respawnDirection = Quaternion.LookRotation(transform.forward);
            StartCoroutine(MoveFloppyDisc());
            //DESIGN HAVE SAID THAT THE PLAYER SHOULD RESPAWN AT A CHECKPOINT WITH THE NUMBER OF POINTS THEY HAD UPON DEATH
            //e.g. if they have 500 points when they cross the checkpoint but have 1000 points when they die, they respawn at
            //the checkpoint with 1000 points
            //therefore the points in the score system don't need to change upon death when the player chooses to respawn at
            //the checkpoint
            m_used = true;
        }
    }
}
