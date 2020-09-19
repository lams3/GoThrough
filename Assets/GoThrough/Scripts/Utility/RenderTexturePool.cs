using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoThrough.Utility
{
    public class RenderTexturePool : ScriptableObject
    {
        public class Item
        {
            public RenderTexture renderTexture;
            public bool used;
        }

        public int Width
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

        public int Height
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

        public int MaxTextureAllocations { get; set; }

        private int width;
        private int height;
        private List<Item> itens = new List<Item>();

        public static RenderTexturePool Create(int width, int height, int maxTextureAllocations)
        {
            RenderTexturePool pool = CreateInstance<RenderTexturePool>();
            pool.Width = width;
            pool.Height = height;
            pool.MaxTextureAllocations = maxTextureAllocations;
            return pool;
        }

        private void OnDestroy()
        {
            foreach (Item item in this.itens)
                DestroyRenderTexture(item);
        }

        public void SetResolution(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public Item GetRenderTexture()
        {
            foreach (Item item in this.itens)
                if (!item.used)
                {
                    item.used = true;
                    return item;
                }

            if (this.itens.Count >= this.MaxTextureAllocations)
                throw new OverflowException("GoThrough's max render texture pool capacity reached.");

            Item newItem = this.CreateRenderTexture();
            this.itens.Add(newItem);
            newItem.used = true;
            return newItem;
        }

        public void ReleaseRenderTexture(Item item)
        {
            item.used = false;
        }

        public void ReleaseAllRenderTextures()
        {
            foreach (Item item in this.itens)
                this.ReleaseRenderTexture(item);
        }

        private Item CreateRenderTexture()
        {
            var newRenderTexture = new RenderTexture(this.width, this.height, 24, RenderTextureFormat.DefaultHDR);
            newRenderTexture.Create();
            newRenderTexture.name = $"Temp Pool Buffer {this.itens.Count}";

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
            foreach (Item item in this.itens)
                DestroyRenderTexture(item);
            this.itens.Clear();
        }
    }
}