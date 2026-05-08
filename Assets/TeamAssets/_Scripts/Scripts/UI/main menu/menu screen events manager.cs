using System;
using UnityEngine;

public class menuscreeneventsmanager : MonoBehaviour
{
    public Action IsVisible;
    public Action IsInvisible;

    private void OnEnable()
    {
        Debug.Log("enabled");
        try
        {
            IsVisible.Invoke();
        }
        catch (Exception e)
        {

        }
    }

    private void OnDisable()
    {
        try
        {
            IsInvisible.Invoke();
        }
        catch (Exception e)
        {

        }
    }
}
