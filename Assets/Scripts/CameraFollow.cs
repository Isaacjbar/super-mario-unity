using UnityEngine;

/// <summary>
/// Side-scrolling camera: follows the player to the right only (classic Mario) and never leaves the level.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform Target;

    /// <summary>Left edge of the visible area, used to stop the player from walking back off-screen.</summary>
    public static float LeftEdge { get; private set; }

    float HalfWidth => Camera.main.orthographicSize * Camera.main.aspect;

    public void Snap()
    {
        var p = transform.position;
        p.x = HalfWidth - 0.5f;
        transform.position = p;
        LeftEdge = p.x - HalfWidth;
    }

    void LateUpdate()
    {
        if (Target == null) return;
        float minX = transform.position.x;
        float maxX = LevelBuilder.Width - 0.5f - HalfWidth;
        float x = Mathf.Clamp(Mathf.Max(minX, Target.position.x + 2f), HalfWidth - 0.5f, Mathf.Max(maxX, HalfWidth - 0.5f));
        transform.position = new Vector3(x, transform.position.y, -10f);
        LeftEdge = x - HalfWidth;
    }
}
