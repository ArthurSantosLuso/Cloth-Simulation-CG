using UnityEngine;

/// <summary>
/// Represents a single point in the cloth mesh - Particle.
/// Responsible only for storing position and handling Verlet integration.
/// </summary>
[System.Serializable]
public class ClothParticle
{
    public Vector3 position;
    public Vector3 prevPosition;
    public Vector3 originalPos;
    public bool isPinned;

    private Vector3 _acceleration;

    public ClothParticle(Vector3 pos, bool isPinned)
    {
        position = pos;
        prevPosition = pos;
        originalPos = pos;
        this.isPinned = isPinned;
        _acceleration = Vector3.zero;
    }

    public void AddForce(Vector3 force)
    {
        _acceleration += force;
    }

    /// <summary>
    /// Verlet Integration - moves the particle based on inertia (pos - prevPos).
    /// </summary>
    public void TimeStep(float dt, float damping)
    {
        if (isPinned) return;

        Vector3 velocity = (position - prevPosition) * (1f - damping);
        Vector3 nextPos = position + velocity + _acceleration * (dt * dt);

        prevPosition = position;
        position = nextPos;
        _acceleration = Vector3.zero; // Reset acceleration for next frame
    }
}