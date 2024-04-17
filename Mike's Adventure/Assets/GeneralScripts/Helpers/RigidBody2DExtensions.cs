using UnityEngine;

public static class RigidBody2DExtensions
{
    public static void AdjustVelocityX(this Rigidbody2D rigidbody, float newX)
    {
        rigidbody.velocity = new Vector2(newX, rigidbody.velocity.y);
    }

    public static void AdjustVelocityY(this Rigidbody2D rigidbody, float newY)
    {
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, newY);
    }
}
