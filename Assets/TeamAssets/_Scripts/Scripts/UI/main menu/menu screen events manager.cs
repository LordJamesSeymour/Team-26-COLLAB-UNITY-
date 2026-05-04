using System;
using UnityEngine;

public class menuscreeneventsmanager : MonoBehaviour
{
    public Action IsVisible;
    public Action IsInvisible;

    private void OnEnable()
    {
        Debug.Log("enabled");
        IsVisible.Invoke();
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
