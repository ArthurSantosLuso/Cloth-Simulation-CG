using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements.Experimental;

/// <summary>
/// Represents a spring constraint between TWO cloth particle.
/// Tries to keep particles close to their rest lenght.
/// </summary>
public class ClothSpring : MonoBehaviour
{
    public ClothParticle particleA;
    public ClothParticle particleB;

    public float restLenght;
    public float stiffness;

    public ClothSpring(ClothParticle a, ClothParticle b, float stiffness)
    {
        particleA = a;
        particleB = b;
        restLenght = Vector3.Distance(a.position, b.position);
        this.stiffness = stiffness;
    }

    /// <summary>
    /// Enforces the distance constraint between the two particles.
    /// </summary>
    public void ApplyConstrains()
    {
        Vector3 delta = particleB.position - particleA.position;
        float currentLenght = delta.magnitude;

        if (currentLenght == 0) return;

        float difference = (currentLenght - restLenght) / currentLenght;
        Vector3 correction = delta * 0.5f * stiffness * difference;

        if (!particleA.isFixed)
            particleA.position += correction;

        if (!particleB.isFixed)
            particleB.position -= correction;
    }
}
