using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Represents a single mass point in the cloth.
/// Uses Verlet integration for real-time physics.
/// </summary>
public class ClothParticle
{
    public Vector3 position;
    public Vector3 previousPosition;
    public Vector3 acceleration;

    public float mass;
    public bool isFixed;

    public ClothParticle(Vector3 startPosition, float mass, bool isFixed = false)
    {
        position = startPosition;
        previousPosition = startPosition;
        acceleration = Vector3.zero;
        this.mass = mass;
        this.isFixed = isFixed;
    }

    /// <summary>
    /// Adds a force to the particle
    /// </summary>
    public void AddForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    /// <summary>
    /// Verlet is applied here.
    /// </summary>
    public void UpdateParticle(float deltaTime)
    {
        if (isFixed) return;

        Vector3 velocity = position - previousPosition;
        Vector3 newPosition = position + velocity + acceleration * Mathf.Pow(deltaTime, 2);

        previousPosition = position;
        position = newPosition;
        acceleration = Vector3.zero;
    }
}
