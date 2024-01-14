using UnityEngine;

public class PredictedCollision
{
    public MyNavMeshAgent collisionObject;
    public float firstCollisionAtFrame;  // time
    const int FramesNeededForValidCollision = 3;
    // int _consecutiveFrames;
    public bool ongoingCollision;

    public PredictedCollision(MyNavMeshAgent obj)
    {
        collisionObject = obj;
        firstCollisionAtFrame = Time.frameCount;
        ongoingCollision = true;
    }
}
