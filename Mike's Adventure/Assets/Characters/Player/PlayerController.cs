using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector2 _movementInput = Vector2.zero;

    [Tooltip("Determines the maximum run-speed")]
    public float MaxRunSpeed = 10f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        Debug.Assert(MaxRunSpeed > 0, "'MaxRunSpeed' must be greater than 0!");
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(_movementInput.x * MaxRunSpeed, _rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }
}
