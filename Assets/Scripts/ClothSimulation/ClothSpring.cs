using UnityEngine;

/// <summary>
/// Represents a constraint between two particles.
/// Responsible for maintaining the correct distance between them.
/// </summary>
public class ClothSpring
{
    private ClothParticle _p1;
    private ClothParticle _p2;
    private float _restLength;

    public ClothSpring(ClothParticle p1, ClothParticle p2)
    {
        _p1 = p1;
        _p2 = p2;
        _restLength = Vector3.Distance(p1.position, p2.position);
    }

    public void Resolve(float stiffness)
    {
        Vector3 delta = _p2.position - _p1.position;
        float currentDist = delta.magnitude;

        // Prevent division by zero and unnecessary processing
        if (currentDist < 0.00001f) return;

        float error = (currentDist - _restLength) / currentDist;

        // Correction movement (half for each side)
        Vector3 correction = delta * 0.5f * error * stiffness;

        if (!_p1.isPinned && !_p2.isPinned)
        {
            _p1.position += correction;
            _p2.position -= correction;
        }
        else if (!_p1.isPinned)
        {
            _p1.position += correction * 2f;
        }
        else if (!_p2.isPinned)
        {
            _p2.position -= correction * 2f;
        }
    }
}
