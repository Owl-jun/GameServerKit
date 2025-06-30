using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string clientId;
    public static string userId;
    public static string token;

    void Awake()
    {
        Application.runInBackground = true;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 넘어가도 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        clientId = "User_" + Guid.NewGuid().ToString().Substring(0, 8);
        Debug.Log("Client ID: " + clientId);
    }
}

