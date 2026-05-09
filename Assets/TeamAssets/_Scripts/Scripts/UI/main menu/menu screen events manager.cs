using System;
using UnityEngine;

public class menuscreeneventsmanager : MonoBehaviour
{
    public Action IsVisible;

    private void OnEnable()
    {
        Debug.Log("enabled");
        IsVisible.Invoke();
    }
}
