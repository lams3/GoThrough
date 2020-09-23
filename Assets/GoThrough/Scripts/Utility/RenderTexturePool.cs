using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoThrough.Utility
{
    /// <summary>
    /// A dynamic resolution RenderTexturePool.
    /// </summary>
    internal class RenderTexturePool : ScriptableObject
    {
        #region PrivateFields
        
        private int width;
        private int height;
        private List<Item> items = new List<Item>();

        #endregion

        #region PublicProperties

        /// <summary>
        /// The width of the allocatedTextures. Changing this value will cause realocation of the textures.
        /// </summary>
        internal int Width
        {
            get => this.width;
            set {
                if (value != this.width)
                {
                    this.width = value;
                    this.Clear();
                }
            }
        }

        /// <summary>
        /// The height of the allocatedTextures. Changing this value will cause realocation of the textures.
        /// </summary>
        internal int Height
        {
            get => this.height;
            set
            {
                if (value != this.height)
                {
                    this.height = value;
                    this.Clear();
                }
            }
        }

        /// <summary>
        /// The maximum number of textures allowed.
        /// </summary>
        internal int MaxTextureAllocations { get; set; }

        #endregion

        #region Lyfecycle

        private void OnDestroy()
        {
            foreach (Item item in this.items)
                DestroyRenderTexture(item);
        }

        #endregion

        #region InternalMethods

        /// <summary>
        /// Creates a new RenderTexturePool.
        /// </summary>
        /// <param name="width">The width of the allocated textures.</param>
        /// <param name="height">The height of the allocated textures.</param>
        /// <param name="maxTextureAllocations">The maximum number of textures allocated.</param>
        /// <returns>The created RenderTexturePool.</returns>
        internal static RenderTexturePool Create(int width, int height, int maxTextureAllocations)
        {
            RenderTexturePool pool = CreateInstance<RenderTexturePool>();
            pool.Width = width;
            pool.Height = height;
            pool.MaxTextureAllocations = maxTextureAllocations;
            return pool;
        }

        /// <summary>
        /// Sets the width and height of the textures. Will cause realocation.
        /// </summary>
        /// <param name="width">The new texture width.</param>
        /// <param name="height">The new texture height.</param>
        internal void SetResolution(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Gets a texture from the pool.
        /// </summary>
        /// <returns>A free pool item.</returns>
        internal Item GetRenderTexture()
        {
            foreach (Item item in this.items)
                if (!item.used)
                {
                    item.used = true;
                    return item;
                }

            if (this.items.Count >= this.MaxTextureAllocations)
                throw new OverflowException("GoThrough's max render texture pool capacity reached.");

            Item newItem = this.CreateRenderTexture();
            this.items.Add(newItem);
            newItem.used = true;
            return newItem;
        }

        /// <summary>
        /// Releases a texture to be used.
        /// </summary>
        /// <param name="item">The pool item to be released.</param>
        internal void ReleaseRenderTexture(Item item)
        {
            item.used = false;
        }

        /// <summary>
        /// Release all used textures.
        /// </summary>
        internal void ReleaseAllRenderTextures()
        {
            foreach (Item item in this.items)
                this.ReleaseRenderTexture(item);
        }

        #endregion

        #region PrivateMethods

        private Item CreateRenderTexture()
        {
            var newRenderTexture = new RenderTexture(this.width, this.height, 24, RenderTextureFormat.DefaultHDR);
            newRenderTexture.Create();
            newRenderTexture.name = $"Temp Pool Buffer {this.items.Count}";

            return new Item
            {
                renderTexture = newRenderTexture,
                used = false
            };
        }

        private void DestroyRenderTexture(Item item)
        {
            item.renderTexture.Release();
            Destroy(item.renderTexture);
        }

        private void Clear()
        {
            foreach (Item item in this.items)
                DestroyRenderTexture(item);
            this.items.Clear();
        }

        #endregion

        #region InnerTypes

        /// <summary>
        /// An item of the pool.
        /// </summary>
        internal class Item
        {
            internal RenderTexture renderTexture;
            internal bool used;
        }

        #endregion
    }
}