using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public InputActionReference MoveActionRef { get; private set; }
    [field: SerializeField] public InputActionReference InteractActionRef { get; private set; }

    Rigidbody2D rb;

    GameObject InteractObject { get; set; }
    Portal Portal { get; set; }

    private void OnEnable()
    {
        MoveActionRef.action.Enable();
        InteractActionRef.action.Enable();
    }

    private void OnDisable()
    {
        MoveActionRef.action.Disable();
        InteractActionRef.action.Disable();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        InteractActionRef.action.performed += _ =>
        {
            Debug.Log("E");
            if(Portal != null)
            {
                Portal.Teleport(transform);
            }
            else
            {
                Debug.Log("can't use anything.");
            }
        };
    }

    private void Update()
    {
        var move = MoveActionRef.action.ReadValue<Vector2>();

        rb.velocity = Speed * Time.deltaTime * move.normalized;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Portal"))
        {
            if(collision.TryGetComponent(out Portal portal))
            {
                Portal = portal;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Portal"))
        {
            if (collision.TryGetComponent(out Portal portal))
            {
                if(Portal == portal)
                {
                    Portal = null;
                }
            }
        }
    }


}
