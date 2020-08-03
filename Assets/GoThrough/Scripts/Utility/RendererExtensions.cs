using UnityEngine;

namespace GoThrough.Utility
{
	public static class RendererExtensions
	{
		public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
		{
			Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
			return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
		}
	}
}