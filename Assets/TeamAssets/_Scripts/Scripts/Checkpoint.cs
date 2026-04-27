using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Death m_playerDeathScript;

    [HideInInspector] public bool m_used;
    public static bool m_checkpointsEnabled;    //this is static as if one checkpoint is disabled, all of them are
                                                //this variable is the one set in the settings menu when the player decides if
                                                //checkpoints are enabled or not

    private void Awake()
    {
        m_checkpointsEnabled = true;    //REMOVE THIS LINE WHEN SETTINGS MENU IS MADE
        m_used = false;
        if(!m_checkpointsEnabled) gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && m_used == false)
        {
            m_playerDeathScript.m_respawnPoint = transform.position;
            m_playerDeathScript.m_respawnPoint.y = transform.position.y + 0.841f;
            //DESIGN HAVE SAID THAT THE PLAYER SHOULD RESPAWN AT A CHECKPOINT WITH THE NUMBER OF POINTS THEY HAD UPON DEATH
            //e.g. if they have 500 points when they cross the checkpoint but have 1000 points when they die, they respawn at
            //the checkpoint with 1000 points
            //therefore the points in the score system don't need to change upon death when the player chooses to respawn at
            //the checkpoint
            m_used = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
