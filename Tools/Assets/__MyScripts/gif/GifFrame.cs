/*
 * Copyright (c) 2015 Thomas Hourdel
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 *    https://github.com/trarck/UnityGif/blob/master/Assets/Scripts/Gif/GifFrame.cs
 */

using UnityEngine;

namespace Gif
{
	public class GifFrame
	{
		public int width;
		public int height;
		public Color32[] pixels;

        public GifFrame()
        {

        }

        public GifFrame(int width,int height)
        {
            this.width = width;
            this.height = height;
            pixels = new Color32[width * height];
        }

        public void SetPixel(uint x,uint y,Color32 color)
        {
            pixels[y * width + x] = color;
        }

        public void SetPixel(Color32[] color)
        {
            pixels = color;
        }

        public void SetPixelEx(uint x, uint y, Color32 color)
        {
            pixels[(height-y-1) * width + x] = color;
        }

        public void Clear(Color color)
        {
            for(uint i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }
        }
    }
}
