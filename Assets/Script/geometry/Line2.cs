/*
using UnityEngine;


public class Line2
{
    Vector3 Start { get; set; }
    Vector3 End { get; set; }
    Vector3 DirectionVector { get; set; }
    float _k;  // k in equation y = kx + q

    float _a;  // a in equation y = ax + b
    float _b;  // b in equation y = ax + b

    public Line2(Vector2 point1, Vector2 point2)
    {
        Start = point1;
        End = point2;
        DirectionVector = point2 - point1;
        // Zde jsem skončil. Potřebuji poskládat obecnou rovnici.
        // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        // "Hence, the point of intersection is" (v článku)
        _k = DirectionVector.y / DirectionVector.y;
    }

    // TODO: // Potřebuju collision point

    public static bool VsCircle(Line2 line, Circle circle) {
        if (pointInCircle(line.getStart(), circle) || pointInCircle(line.getEnd(), circle)) {
            return true;
        }

        Vector2f ab = new Vector2f(line.getEnd()).sub(line.getStart());

        // Project point (circle position) onto ab (line segment)
        // parameterized position t
        Vector2f circleCenter = circle.getCenter();
        Vector2f centerToLineStart = new Vector2f(circleCenter).sub(line.getStart());
        float t = centerToLineStart.dot(ab) / ab.dot(ab);

        if (t < 0.0f || t > 1.0f) {
            return false;
        }

        // Find the closest point to the line segment
        Vector2f closestPoint = new Vector2f(line.getStart()).add(ab.mul(t));

        return pointInCircle(closestPoint, circle);
    }    
}
*/