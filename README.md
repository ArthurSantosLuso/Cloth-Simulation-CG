# Cloth-Simulation-CG
Repository for Computer Graphics final project. Cloth Simulation


# Report - Custom Cloth Simulation in Unity

## 1. Introduction

This project presents the development of a **custom cloth simulation system** implemented in Unity, without using the built-in `Cloth` component. The main objective is to study, design, and implement a physically inspired real-time cloth solver, demonstrating understanding of numerical integration, constraint-based dynamics, and collision handling. 

This project was developed by Arthur Santos (a22503968) for the Computer Graphics (CG) class and utilizes Unity version 6000.0.63f1.

The project focuses on:
- Particle-based cloth representation
- Verlet integration
- Distance constraints (springs)
- Iterative constraint solving
- Collision handling with spheres
- Self-collision using spatial hashing

---


## 2. Related Work and Research

There are many ways to implement cloth simulation; among the most common are mass-spring system and position-based dynamics (PBD) techniques. The mass-spring system technique was chosen for this project because it is "simpler" and much more widely used in games. Some of the primary sources:<br>
[Guthub Project](https://roryvaughn.github.io/Cloth-Physics/)<br>
[Paul G. Allen School](https://courses.cs.washington.edu/courses/cse457/25au/project/cloth/)<br>
[Paper](https://www.mdpi.com/2076-3417/11/17/8255)

---

## 3. Cloth Representation

The cloth is represented as a **regular grid of particles**, where each particle corresponds to a vertex of a dynamically generated mesh.

- Each particle stores it's current and previous positions
- The top row of particles is pinned, creating a hanging cloth
- The visual mesh is updated every frame using particle positions

This approach follows a mass-spring and Position-Based Dynamics (PBD) formulations that is commonly used in real-time cloth simulation.

---

## 4. Particle Integration - Verlet Integration

Particle motion is integrated using **Verlet integration**, which is good for constraint-based simulations due to its numerical stability and implicit velocity handling.

The particle position is updated using:

```
velocity ≈ currentPosition − previousPosition
nextPosition = currentPosition + velocity + acceleration × dt²
```

Damping is applied directly to the implicit velocity to simulate air resistance and reduce numerical energy accumulation.

---

## 5. Constraint System (Springs)

The cloth structure is maintained using **distance constraints - springs -** between particles.

### 5.1 Structural Springs

Structural springs connect:
- Horizontal neighbors
- Vertical neighbors

They preserve the basic shape and prevent tearing.

### 5.2 Shear Springs

Shear springs connect diagonal neighbors in the grid. Their purpose is to:
- Prevent excessive shearing
- Preserve the rectangular structure of the mesh

### 5.3 Constraint Resolution

Each spring enforces its rest length using positional corrections. If one particle is pinned, the full correction is applied to the free particle. This prevents energy loss near fixed constraints and improves realism.

The solver runs **multiple iterations per frame**, gradually converging toward constraint satisfaction.

---

## 6. Solver Architecture (Position-Based Dynamics)

The simulation follows a **Position-Based Dynamics (PBD)** approach:

1. Apply external forces (gravity)
2. Integrate particle positions (Verlet)
3. Iteratively solve constraints:
   - Spring constraints
   - External collisions
   - Self-collisions

Increasing the number of solver iterations improves stiffness and collision robustness at the cost of performance.

---

## 7. Collision Handling

### 7.1 Sphere Collision

Collision with spheres is implemented by:
- Computing the vector from the sphere center to each particle
- Checking penetration using squared distances
- Projecting particles back to the sphere surface

A small collision margin is added to reduce visual clipping.

This collision handling is executed inside the solver loop, ensuring stability and reducing tunneling effects.

### 7.2 Self-Collision

Self-collision prevents the cloth from intersecting itself. A naive O(n²) approach is too expensive, so **spatial hashing** is used.

#### Spatial Hashing

- Space is divided into 3D grid cells
- Each particle is assigned to a cell
- Collisions are only tested between particles within the same cell

This reduces computational complexity to near-linear time and allows real-time self-collision handling.

---

## 8. Mesh Generation and Rendering

The cloth mesh is generated procedurally at runtime:
- Vertices correspond to particles
- Triangles and UVs are created once during initialization
- Vertex positions are updated every frame

`Mesh.MarkDynamic()` is used to inform Unity that the mesh will be updated frequently, improving performance.

---

## 9. Results and Observations

The final system demonstrates:
- Stable hanging cloth behavior
- Realistic deformation under gravity
- Robust collision with spheres
- Effective self-collision prevention

Limitations include:
- No advanced bending constraints
- No continuous collision detection (CCD)
- No interaction with arbitrary mesh colliders

Despite these limitations, the system successfully demonstrates the fundamental principles of cloth simulation.

---

## 10. Conclusion

This project demonstrates that a functional and stable cloth simulation can be implemented from scratch using relatively simple techniques when grounded in correct physical and numerical principles.

The use of Verlet integration, iterative constraint solving, and spatial hashing shows clear research effort and understanding of real-time simulation techniques, fulfilling the academic objectives of the assignment.

---

## 11. Bibliography

- Jakobsen, T. (2001). *Advanced Character Physics*. GDC.
  https://www.gdcvault.com/play/1020615/Advanced-Character-Physics

- Müller, M., Heidelberger, B., Hennix, M., & Ratcliff, J. (2007). *Position Based Dynamics*. Journal of Visual Communication and Image Representation.
  https://matthias-research.github.io/pages/publications/posBasedDyn.pdf

- Unity Technologies. *Unity Cloth Manual*.
  https://docs.unity3d.com/Manual/class-Cloth.html

- Espinosa, A. *Cloth Behaviour Simulation* (GitHub Repository).
  https://github.com/AEspinosaDev/Cloth-Behaviour-Simulation

- Baraff, D., & Witkin, A. (1998). *Large Steps in Cloth Simulation*. SIGGRAPH.
  https://www.cs.cmu.edu/~baraff/papers/sig98.pdf
