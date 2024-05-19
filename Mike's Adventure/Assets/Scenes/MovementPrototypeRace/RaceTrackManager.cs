using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class RaceTrackManager : MonoBehaviour
{
    private readonly Stopwatch _stopwatch = new();
    private readonly Collider2D[] colliders = new Collider2D[1];

    [Tooltip("The detector for when the timer should start")]
    public Collider2D StartCollider;
    [Tooltip("The detector for when the timer should stop")]
    public Collider2D EndCollider;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(StartCollider != null, "'StartCollider' must be set!");
        Debug.Assert(EndCollider != null, "'EndCollider' must be set!");
    }

    void FixedUpdate()
    {
        if(StartCollider.GetContacts(colliders) > 0)
        {
            _stopwatch.Restart();
        }

        if(EndCollider.GetContacts(colliders) > 0)
        {
            _stopwatch.Stop();
            Debug.Log($"You ran the course in {_stopwatch.Elapsed}!");
        }
    }
}
