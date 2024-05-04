using UnityEngine;

/// <summary>
/// Uses the collider to check directions to see if the object is currently on
/// the ground, touching a wall or touching a ceiling.
/// </summary>
[RequireComponent(typeof(CapsuleCollider2D))]
public class TouchingDirections : MonoBehaviour, ITouchingDirections
{
    private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[5];
    private readonly RaycastHit2D[] _wallHits = new RaycastHit2D[5];
    private readonly RaycastHit2D[] _ceilingHits = new RaycastHit2D[5];
    private CapsuleCollider2D _touchingCollider;

    [Tooltip("Configures which raycast hits are within consideration")]
    public ContactFilter2D CastFilter;

    [Tooltip("Determines the tolerance at which touching the ground is considered happening")]
    public float GroundDistance = 0.05f;
    [Tooltip("Determines the tolerance at which touching a wall is considered happening")]
    public float WallDistance = 0.2f;
    [Tooltip("Determines the tolerance at which touching the ceiling is considered happening")]
    public float CeilingDistance = 0.05f;

    [field: SerializeField, ReadOnlyField, Tooltip("Indicates if the character is grounded or not.")]
    public bool IsGrounded { get; private set; }

    [field: SerializeField, ReadOnlyField, Tooltip("Indicates if the character is touching a wall or not.")]
    public bool IsOnWall { get; private set; }

    [field: SerializeField, ReadOnlyField, Tooltip("Indicates what wall the character is touching.")]
    public WallDirection WallDirection { get; private set; }

    [field: SerializeField, ReadOnlyField, Tooltip("Indicates if the character is obstructed above them or not.")]
    public bool IsOnCeiling { get; private set; }

    private Vector2 WallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

    private void Awake()
    {
        Debug.Assert(GroundDistance > 0, "'GroundDistance' must be greater than 0!");
        Debug.Assert(WallDistance > 0, "'WallDistance' must be greater than 0!");
        Debug.Assert(CeilingDistance > 0, "'CeilingDistance' must be greater than 0!");
        _touchingCollider = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        IsGrounded = _touchingCollider.Cast(Vector2.down, CastFilter, _groundHits, GroundDistance) > 0;
        IsOnWall = _touchingCollider.Cast(WallCheckDirection, CastFilter, _wallHits, WallDistance) > 0;
        if (IsOnWall)
        {
            WallDirection = gameObject.transform.localScale.x > 0 ? WallDirection.Right : WallDirection.Left;
        }
        else
        {
            WallDirection = WallDirection.None;
        }
        IsOnCeiling = _touchingCollider.Cast(Vector2.up, CastFilter, _ceilingHits, CeilingDistance) > 0;
    }
}

public interface ITouchingDirections
{
    public bool IsGrounded { get; }
    public bool IsOnWall { get; }
    public bool IsOnCeiling { get; }
    public WallDirection WallDirection { get; }
}

public enum WallDirection
{
    None,
    Right,
    Left
}
