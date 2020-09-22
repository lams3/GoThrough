using UnityEngine;

namespace GoThrough.Utility
{
    /// <summary>
    /// Some helper methods for UnityEngine.Camera class.
    /// </summary>
    internal static class CameraExtensions
    {
        private static readonly Vector3[] cubeCornerOffsets = {
            new Vector3 (1, 1, 1),
            new Vector3 (-1, 1, 1),
            new Vector3 (-1, -1, 1),
            new Vector3 (-1, -1, -1),
            new Vector3 (-1, 1, -1),
            new Vector3 (1, -1, -1),
            new Vector3 (1, 1, -1),
            new Vector3 (1, -1, 1),
        };

        /// <summary>
        /// Checks if the 2D bounds of <paramref name="nearObject"/> and <paramref name="farObject"/> overlap in the view of <paramref name="camera"/>.
        /// Also checks if <paramref name="nearObject"/> is indeed closer to <paramref name="camera"/> than <paramref name="farObject"/>.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="nearObject">The MeshFilter closer to <paramref name="camera"/>.</param>
        /// <param name="farObject">The other MeshFilter.</param>
        /// <returns>true if bounds overlap and <paramref name="nearObject"/> is closer.</returns>
        internal static bool BoundsOverlap(this Camera camera, MeshFilter nearObject, MeshFilter farObject)
        {
            var near = camera.GetScreenRectFromBounds(nearObject);
            var far = camera.GetScreenRectFromBounds(farObject);

            // ensure far object is indeed further away than near object
            if (far.zMax > near.zMin)
            {
                // Doesn't overlap on x axis
                if (far.xMax < near.xMin || far.xMin > near.xMax)
                {
                    return false;
                }
                // Doesn't overlap on y axis
                if (far.yMax < near.yMin || far.yMin > near.yMax)
                {
                    return false;
                }
                // Overlaps
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets an screen-space boundary from the <paramref name="renderer"/> boundary.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="renderer">The object.</param>
        /// <returns>An screen-space boundary of the object.</returns>
        // With thanks to http://www.turiyaware.com/a-solution-to-unitys-camera-worldtoscreenpoint-causing-ui-elements-to-display-when-object-is-behind-the-camera/
        internal static MinMax3D GetScreenRectFromBounds(this Camera camera, MeshFilter renderer)
        {
            MinMax3D minMax = new MinMax3D(float.MaxValue, float.MinValue);

            Vector3[] screenBoundsExtents = new Vector3[8];
            var localBounds = renderer.sharedMesh.bounds;
            bool anyPointIsInFrontOfCamera = false;

            for (int i = 0; i < 8; i++)
            {
                Vector3 localSpaceCorner = localBounds.center + Vector3.Scale(localBounds.extents, cubeCornerOffsets[i]);
                Vector3 worldSpaceCorner = renderer.transform.TransformPoint(localSpaceCorner);
                Vector3 viewportSpaceCorner = camera.WorldToViewportPoint(worldSpaceCorner);

                if (viewportSpaceCorner.z > 0)
                {
                    anyPointIsInFrontOfCamera = true;
                }
                else
                {
                    // If point is behind camera, it gets flipped to the opposite side
                    // So clamp to opposite edge to correct for this
                    viewportSpaceCorner.x = (viewportSpaceCorner.x <= 0.5f) ? 1 : 0;
                    viewportSpaceCorner.y = (viewportSpaceCorner.y <= 0.5f) ? 1 : 0;
                }

                // Update bounds with new corner point
                minMax.AddPoint(viewportSpaceCorner);
            }

            // All points are behind camera so just return empty bounds
            if (!anyPointIsInFrontOfCamera)
            {
                return new MinMax3D();
            }

            return minMax;
        }

        internal struct MinMax3D
        {
            internal float xMin;
            internal float xMax;
            internal float yMin;
            internal float yMax;
            internal float zMin;
            internal float zMax;

            internal MinMax3D(float min, float max)
            {
                this.xMin = min;
                this.xMax = max;
                this.yMin = min;
                this.yMax = max;
                this.zMin = min;
                this.zMax = max;
            }

            internal void AddPoint(Vector3 point)
            {
                xMin = Mathf.Min(xMin, point.x);
                xMax = Mathf.Max(xMax, point.x);
                yMin = Mathf.Min(yMin, point.y);
                yMax = Mathf.Max(yMax, point.y);
                zMin = Mathf.Min(zMin, point.z);
                zMax = Mathf.Max(zMax, point.z);
            }
        }
    }
}