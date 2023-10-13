using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FinishLine : MonoBehaviour
{
    Collider2D Collider { get; set; }

    private void Awake()
    {
        Collider = GetComponent<Collider2D>();
        
        // make sure collider is trigger.
        Collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hooray! You've solved this maze, and we have over 1000 more mazes waiting for you.");
    }
}
