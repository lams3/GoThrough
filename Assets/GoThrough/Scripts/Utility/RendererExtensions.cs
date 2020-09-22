using UnityEngine;

namespace GoThrough.Utility
{
	/// <summary>
	/// Some helper methods for UnityEngine.Renderer class.
	/// </summary>
	internal static class RendererExtensions
	{
		/// <summary>
		/// Tests <paramref name="renderer"/> bounds against <paramref name="camera"/> frustum planes.
		/// </summary>
		/// <param name="renderer">The Renderer.</param>
		/// <param name="camera">The Camera.</param>
		/// <returns>True if <paramref name="renderer"/> intersects with <paramref name="camera"/> frustum.</returns>
		internal static bool IsVisibleFrom(this Renderer renderer, Camera camera)
		{
			Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
			return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
		}
	}
}