using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum CollisionDirection
{
    None,
    Horizontal,
    Vertical,
}

public class Physics : MonoBehaviour
{
    // Array that stores colliders the box comes into contact with
    Collider2D[] results;

    // Offset between the corner of the collider and where the first ray is sent out from.
    // Fixes some edge cases where Impact() wouldn't know whether the collision is vertical or horizontal.
    // Makes effective 'collider' smaller, needs to be synced up with smaller Grounded() box (NOT CURRENTLY IMPLEMENTED).
    public float rayOffset = 0.01f;

    //filter for filtering out types of colliders that player can come into contact with
    //can use to, for example, filter obstacle vs wall colliders for different behaviors
    //(currently just ignored, as no colliders have a filter yet)
    //ContactFilter2D filter;

    public float distanceToCheck = 0.25f;

    void Start()
    {
        results = new Collider2D[1];
    }

    void Update() { }

    /// <summary>
    /// Returns the collider that OverlapBox hits, or null if nothing is hit.
    /// </summary>
    public Collider2D Grounded(Vector2 boxPoint, Vector2 boxSize, float boxAngle)
    {
        if (
            Physics2D.OverlapBox(boxPoint, boxSize, boxAngle, ContactFilter2D.noFilter, results) > 0
        )
        {
            return results[0];
        }

        return null;
    }

    /// <summary>
    /// Sends rays out from an object based on velocity to predict collisions this frame.
    /// Returns the coordinate of the impact point (i.e. where the player should snap to).
    /// If an impact is detected, call this again to check for a second impact in the same frame
    /// (e.g. hitting a corner where both horizontal and vertical velocity cause an impact).
    /// Two checks should be enough for any scenario.
    /// </summary>
    /// <param name="objTransform">Transform of the object checking for collisions (e.g. the player).</param>
    /// <param name="objVelocity">Velocity of the object.</param>
    /// <param name="numRays">Number of rays sent from each side (e.g. 3 sends up to 3 from each edge).</param>
    /// <param name="direction">Returns which axis a collision was detected on: "horizontal", "vertical", or "none".</param>
    public Vector2 Impact(
        Transform objTransform,
        Vector2 objVelocity,
        int numRays,
        out CollisionDirection direction
    )
    {
        // Distance that the closest collider is to the player
        float minDist = Mathf.Infinity;
        // Returns a zero vector if there is no result
        Vector2 returnVector = Vector2.zero;

        // Get height and width beforehand for readability (corrected for rayOffset)
        float height = objTransform.localScale.y - rayOffset * 2;
        float width = objTransform.localScale.x - rayOffset * 2;

        direction = CollisionDirection.None;

        for (int i = 0; i < numRays; i++)
        {
            // Check for horizontal collisions
            var (horizontalHit, side, vertOffset) = CheckAxis(
                0,
                objVelocity,
                objTransform,
                i,
                height,
                width,
                numRays
            );

            if (horizontalHit)
            {
                var (newReturn, newMin, updated) = CalculateHit(
                    0,
                    horizontalHit,
                    side,
                    vertOffset,
                    minDist,
                    height,
                    width
                );
                if (updated)
                {
                    returnVector = newReturn;
                    minDist = newMin;
                    direction = CollisionDirection.Horizontal;
                }
            }

            // Check for vertical collisions
            var (verticalHit, vert, horizontalOffset) = CheckAxis(
                1,
                objVelocity,
                objTransform,
                i,
                height,
                width,
                numRays
            );

            if (verticalHit)
            {
                var (newReturn, newMin, updated) = CalculateHit(
                    1,
                    verticalHit,
                    vert,
                    horizontalOffset,
                    minDist,
                    height,
                    width
                );
                if (updated)
                {
                    returnVector = newReturn;
                    minDist = newMin;
                    direction = CollisionDirection.Vertical;
                }
            }
        }

        return returnVector;
    }

    /// <summary>
    /// Casts a ray along the given axis and returns the hit info.
    /// Axis: 0 for horizontal, 1 for vertical.
    /// </summary>
    private (RaycastHit2D hit, int dir, float offset) CheckAxis(
        int axis,
        Vector2 objVelocity,
        Transform objTransform,
        int i,
        float height,
        float width,
        int numRays
    )
    {
        if (objVelocity[axis] != 0)
        {
            float maxDist = objVelocity.magnitude;

            // Get direction (up/down or left/right)
            int dir = objVelocity[axis] < 0 ? -1 : 1;

            // Size of the player on the axis in the direction we are moving
            float movementSize = axis == 0 ? width : height;

            // Size of the player on the axis the rays will be spread across
            float spreadSize = axis == 0 ? height : width;

            float offset = -(spreadSize / 2.0f) + (spreadSize / (numRays - 1) * i);

            // Calculate origin
            Vector2 origin = Vector2.zero;
            origin[axis] = objTransform.position[axis] + (movementSize / 2.0f * dir);
            origin[1 - axis] = objTransform.position[1 - axis] + offset;

            RaycastHit2D hit = Physics2D.Raycast(origin, objVelocity, maxDist);
            return (hit, dir, offset);
        }

        return (default, 0, 0);
    }

    /// <summary>
    /// Calculates the snap position from a raycast hit if it's closer than the current minimum.
    /// Axis: 0 for horizontal, 1 for vertical.
    /// </summary>
    private (Vector2 collision, float minDist, bool updated) CalculateHit(
        int axis,
        RaycastHit2D hit,
        int dir,
        float offset,
        float currentMin,
        float height,
        float width
    )
    {
        if (currentMin > hit.distance)
        {
            // Get point where that ray intersected with a collider
            Vector2 hitPoint = hit.point;
            float otherCoord = hitPoint[1 - axis] - offset;
            Vector2 returnVector =
                axis == 0
                    ? new Vector2(hitPoint[axis] - (width / 2 * dir), otherCoord)
                    : new Vector2(otherCoord, hitPoint[axis] - (height / 2 * dir));

            return (returnVector, hit.distance, true);
        }

        return (default, currentMin, false);
    }
}
