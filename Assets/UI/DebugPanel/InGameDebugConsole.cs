using Mirror;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameDebugConsole : Mirror.NetworkBehaviour
{
    public TextMeshProUGUI logText; // jeœli TextMeshPro u¿yj TMP_Text
    public GameObject historyWindow;
    public TextMeshProUGUI historyLogText;
    public int maxLines = 5; //lines count

    private Queue<string> logQueue = new Queue<string>();
    private List<string> allLogs = new List<string>();


    private void Start()
    {
        GameSettings gameSettings = GameSettings.Instance;
        if (gameSettings != null)
            if (gameSettings.GetSettings().inGameConsoleEnable == false)
                gameObject.SetActive(false);
            else
                gameObject.SetActive(true);
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string color = "white";
        if (type == LogType.Warning) return;//color = "yellow";
        if (type == LogType.Error || type == LogType.Exception) color = "red";

        string timestamp = DateTime.Now.ToString("HH:mm:ss"); //timestamp
        string formattedLog = $"[{timestamp}] <color={color}>{logString}</color>";

        allLogs.Add(formattedLog);
        // add new log
        logQueue.Enqueue(formattedLog);

        // delete old logs
        while (logQueue.Count > maxLines)
        {
            logQueue.Dequeue();
        }

        // join logs
        logText.text = string.Join("\n", logQueue);
    }
    public void ShowHistoryWindow()
    {
        if (historyWindow == null || historyLogText == null) return;
        historyLogText.text = string.Join("\n", allLogs);
        RequestPause();
    }
    [Client]
    public void RequestPause()
    {
        CmdRequestPause();
    }
    [Client]
    public void RequestStart()
    {
        CmdRequestStart();
    }
    [Command]
    private void CmdRequestPause()
    {
        RpcSetPause(true);
    }
    [Command]
    private void CmdRequestStart()
    {
        RpcSetPause(false);
    }

    [ClientRpc]
    private void RpcSetPause(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
    }
}
