using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class DebugUIManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Hold the TMP debug text.")]
    private TextMeshProUGUI _debugTextTMP;

    [SerializeField]
    [Tooltip("Hold the TMP for the time.")]
    private TextMeshProUGUI _timeTextTMP;

    [SerializeField]
    [Tooltip("Hold the TMP for the refresh rate [hz].")]
    private TextMeshProUGUI _refreshRateHzTextTMP;

    [SerializeField]
    [Tooltip("Hold the TMP for refresh rate [ms].")]
    private TextMeshProUGUI _refreshRateMsTextTMP;

    [SerializeField]
    [Tooltip("Max logs cached.")]
    private int _maxQueueSize = 15;

    [SerializeField]
    [Tooltip("Whether the list is presented in falling or ascending order.")]
    private bool _newestAtTop = true;

    /// <summary>
    /// Used to maintain status if the event is subscribed to or not to avoid accidental multiple subscriptions.
    /// </summary>
    private bool _isEnabled = false;

    /// <summary>
    /// Cached log entries.
    /// Latest entry at [0].
    /// </summary>
    private List<DebugLog> _logQueue;

    private void OnEnable()
    {
        SetContinuousMode(true);
    }

    private void OnDisable()
    {
        SetContinuousMode(false);
    }

    /// <summary>
    /// Keeps track when the time TMP was last updated.
    /// </summary>
    private float _last = 0;
    private void Update()
    {
        _refreshRateHzTextTMP.text = $"Refresh rate [hz]: {Math.Round(XRDevice.refreshRate)}";
        _refreshRateMsTextTMP.text = $"Refresh rate [ms]: {Math.Round(100 / XRDevice.refreshRate, 2)}";

        // Not sure if needed to check the time since last update. Though process is that this saves on the CPU/GPU slightly.
        if (Time.realtimeSinceStartup - _last > 0.1)
        {
            _timeTextTMP.text = $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}]";
            _last = Time.realtimeSinceStartup;
        }
    }
    private void Awake()
    {
        _logQueue = new List<DebugLog>();
    }

    public void SetContinuousMode(bool b)
    {
        if (_isEnabled != b)
        {
            if (_isEnabled)
                Application.logMessageReceived -= HandleLog;
            else
                Application.logMessageReceived += HandleLog;
            _isEnabled = b;
        }
    }
    
    public void SetNewestOnTop(bool b) 
    {
        _newestAtTop = b; 
        UpdateText(); 
    }

    /// <summary>
    /// Function invoked when new debug log message is registred.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        _logQueue.Add(new DebugLog(DateTime.Now, condition, stackTrace, type));
        if (_logQueue.Count > _maxQueueSize)
            _logQueue.RemoveAt(0);

        UpdateText();
    }

    private void UpdateText()
    {
        _debugTextTMP.text = String.Empty;


        if (_newestAtTop)
            for (int i = _logQueue.Count - 1; i >= 0; i--)
                WriteText(i);
        else 
            for (int i = 0; i < _logQueue.Count; i++)
                WriteText(i);
    }

    private void WriteText(int i)
    {
        _debugTextTMP.text += $"[{_logQueue[i].dateTime.Hour}:{_logQueue[i].dateTime.Minute}:{_logQueue[i].dateTime.Second}] ";
        switch (_logQueue[i].type)
        {
            case LogType.Error:
                _debugTextTMP.text += $"Severity: <color=\"red\"> {_logQueue[i].type} </color>\n ";
                break;
            case LogType.Assert:
                _debugTextTMP.text += $"Severity: <color=\"orange\"> {_logQueue[i].type} </color>\n ";
                break;
            case LogType.Warning:
                _debugTextTMP.text += $"Severity: <color=\"yellow\"> {_logQueue[i].type} </color>\n ";
                break;
            case LogType.Log:
                _debugTextTMP.text += $"Severity: <color=\"white\"> {_logQueue[i].type} </color>\n ";
                break;
            case LogType.Exception:
                _debugTextTMP.text += $"Severity: <color=\"red\"> {_logQueue[i].type} </color>\n ";
                break;
            default:
                break;
        }
        _debugTextTMP.text += $"Condition: {_logQueue[i].condition} \n ";
        _debugTextTMP.text += $"Stack trace: {_logQueue[i].stackTrace} \n";
        _debugTextTMP.text += "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - \n\n";
    }

    public struct DebugLog
    {
        public readonly DateTime dateTime;
        public readonly string condition;
        public readonly string stackTrace;
        public readonly LogType type;

        public DebugLog(DateTime dateTime, string condition, string stackTrace, LogType type)
        {
            this.dateTime = dateTime;
            this.condition = condition;
            this.stackTrace = stackTrace;
            this.type = type;
        }
    }
}
