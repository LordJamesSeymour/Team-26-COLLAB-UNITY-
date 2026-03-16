using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LukeTest : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    bool isHolding;
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        isHolding = true;

    }

    private void Update()
    {
        if (isHolding) 
        {
            //transform.position = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Debug.Log("Grabbed" );
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Enter");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }
}
