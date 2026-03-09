using Group26.Player.Inputs;
using Group26.Player.Movement;
using Unity.VisualScripting;
using UnityEngine;

public class TrickSystem : MonoBehaviour
{
    private InputManager InputManager;
    private PlayerController Playe;

    private void Awake()
    {
        if (InputManager == null) 
            InputManager = GetComponent<InputManager>();
        if (Playe == null) 
            Playe = GetComponent<PlayerController>();
        //Debug.Log(Playe);
    }

    private void OnEnable()
    {
        InputManager.OnTrickPressed += DoATrick;
    }

    private void OnDisable()
    {
        InputManager.OnTrickPressed -= DoATrick;
    }

    public void DoATrick()
    {
        print("Do a Flip!");
        Object component = transform.GetChild(0); //Finds PlayerBody;
        Object orb = GetComponent<Object>();

        PlayerController controller = orb.GetComponent<PlayerController>();
        //bool b = PlayerController..GetComponent<PlayerController>().
        
        if (Playe == null)
        {
            print("PlayerController is null");
        }
        
        if (controller == null)
        {
            print("Controller is null");
        }


        if (component != null)
        {

            Renderer Renderer = component.GetComponent<Renderer>();
            Color color = Renderer.material.color;
            Renderer.material.SetColor("_BaseColor", Color.white);



        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
