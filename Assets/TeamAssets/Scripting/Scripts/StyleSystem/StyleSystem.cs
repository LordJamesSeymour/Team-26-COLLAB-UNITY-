using UnityEngine;

public class StyleSystem : MonoBehaviour
{
    private InputManager m_inputManager;

    private void Awake()
    {
        m_inputManager = GetComponent<InputManager>();

    }

    private void OnEnable()
    {
        m_inputManager.m_styleAction += Trick;
    }

    private void Trick()
    {
        print("Trick");
    }

}
