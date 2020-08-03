using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoThrough.Utility
{
    public class RenderTexturePool : MonoBehaviourSingleton<RenderTexturePool>
    {
        public class Item
        {
            public RenderTexture renderTexture;
            public bool used;
        }

        public int MaxSize => PortalConfig.Instance.maxRenderTextureAllocations;

        private List<Item> pool = new List<Item>();

        private void OnDestroy()
        {
            foreach (Item item in this.pool)
                DestroyRenderTexture(item);
        }

        public Item GetRenderTexture()
        {
            foreach (Item item in this.pool)
                if (!item.used)
                {
                    item.used = true;
                    return item;
                }

            if (this.pool.Count >= this.MaxSize)
                throw new OverflowException("GoThrough's max render texture pool capacity reached.");

            Item newItem = this.CreateRenderTexture();
            this.pool.Add(newItem);
            newItem.used = true;
            return newItem;
        }

        public void ReleaseRenderTexture(Item item)
        {
            item.used = false;
        }

        public void ReleaseAllRenderTextures()
        {
            foreach (Item item in this.pool)
                this.ReleaseRenderTexture(item);
        }

        private Item CreateRenderTexture()
        {
            var newRenderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.DefaultHDR);
            newRenderTexture.Create();
            newRenderTexture.name = $"Temp Pool Buffer {this.pool.Count}";

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
    }
}