using UnityEngine;

public static class Rigidbody2DExtensions
{
    public static void AddForceAI(this Rigidbody2D thisObj, Vector2 value)
    {
        thisObj.AddForce(value, ForceMode2D.Impulse);
    }
}
