using UnityEngine;

public static class Vector3Extensions
{
    /// <summary>
    /// Snap Vector3 to nearest grid position
    /// </summary>
    /// <param name="vector3">Sloppy position</param>
    /// <param name="gridSize">Grid size</param>
    /// <returns>Snapped position</returns>
    public static Vector3 Snap(this Vector3 vector3, float gridSize = 1.0f)
    {
        return new Vector3(
            Mathf.Round(vector3.x / gridSize) * gridSize,
            Mathf.Round(vector3.y / gridSize) * gridSize,
            Mathf.Round(vector3.z / gridSize) * gridSize);
    }

    /// <summary>
    /// Snap Vector3 to nearest grid position with offset
    /// </summary>
    /// <param name="vector3">Sloppy position</param>
    /// <param name="gridSize">Grid size</param>
    /// <returns>Snapped position</returns>
    public static Vector3 SnapOffset(this Vector3 vector3, Vector3 offset, float gridSize = 1.0f)
    {
        Vector3 snapped = vector3 + offset;
        snapped = new Vector3(
            Mathf.Round(snapped.x / gridSize) * gridSize,
            Mathf.Round(snapped.y / gridSize) * gridSize,
            Mathf.Round(snapped.z / gridSize) * gridSize);
        return snapped - offset;
    }
}