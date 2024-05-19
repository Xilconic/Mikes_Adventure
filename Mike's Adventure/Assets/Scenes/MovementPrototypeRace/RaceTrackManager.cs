using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;

using Debug = UnityEngine.Debug;
using System;

public class RaceTrackManager : MonoBehaviour
{
    private readonly Stopwatch _stopwatch = new();
    private readonly Collider2D[] colliders = new Collider2D[1];

    [Tooltip("The detector for when the timer should start")]
    public Collider2D StartCollider;
    [Tooltip("The detector for when the timer should stop")]
    public Collider2D EndCollider;

    [Tooltip("Text used to display the current best time")]
    public TMP_Text CurrentBestTimeValueText;
    [Tooltip("Text used to display the current time")]
    public TMP_Text CurrentTimeValueText;

    private RaceMode _raceMode = RaceMode.NotStarted;
    private TimeSpan _timeToBeat = TimeSpan.FromSeconds(25);

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(StartCollider != null, "'StartCollider' must be set!");
        Debug.Assert(EndCollider != null, "'EndCollider' must be set!");
        Debug.Assert(CurrentBestTimeValueText != null, "'CurrentBestTimeValueText' must be set!");
        Debug.Assert(CurrentTimeValueText != null, "'CurrentTimeValueText' must be set!");

        CurrentBestTimeValueText.text = _timeToBeat.ToString();
    }

    private void Update()
    {
        switch (_raceMode)
        {
            case RaceMode.NotStarted:
                CurrentTimeValueText.text = "Race not started";
                break;
            case RaceMode.Started:
            case RaceMode.Finished:
                CurrentTimeValueText.text = _stopwatch.Elapsed.ToString();
                break;
        }
    }

    void FixedUpdate()
    {
        if(StartCollider.GetContacts(colliders) > 0)
        {
            _stopwatch.Restart();
            _raceMode = RaceMode.Started;
        }

        if(EndCollider.GetContacts(colliders) > 0)
        {
            _stopwatch.Stop();
            _raceMode = RaceMode.Finished;

            if(_stopwatch.Elapsed < _timeToBeat)
            {
                _timeToBeat = _stopwatch.Elapsed;
                CurrentBestTimeValueText.text = _timeToBeat.ToString();
            }
        }
    }

    private enum RaceMode
    {
        NotStarted,
        Started,
        Finished
    }
}
