/////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006, Frank Blumenberg
// 
// See License.txt for complete licensing and attribution information.
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
// 
/////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////
//
// This code is adapted from code in the Endogine sprite engine by Jonas Beckeman.
// http://www.endogine.com/CS/
//
/////////////////////////////////////////////////////////////////////////////////

using System.Drawing;
using System.IO;

namespace PhotoshopFile
{
    /// <summary>
    /// Summary description for Thumbnail.
    /// </summary>
    public class Thumbnail : ImageResource
    {
        public Bitmap Image { get; set; }

        public Thumbnail(ImageResource imgRes) : base(imgRes)
        {
            using (var reader = DataReader)
            {
                var format = reader.ReadInt32();
                var width = reader.ReadInt32();
                var height = reader.ReadInt32();
#if true
                reader.ReadBytes(16);
#else
                var widthBytes = reader.ReadInt32();
                var size = reader.ReadInt32();
                var compressedSize = reader.ReadInt32();
                var bitPerPixel = reader.ReadInt16();
                var planes = reader.ReadInt16();
#endif

                if (format == 1)
                {
                    var imgData = reader.ReadBytes((int) (reader.BaseStream.Length - reader.BaseStream.Position));

                    using (var strm = new MemoryStream(imgData))
                    {
                        Image = (Bitmap)(Bitmap.FromStream(strm).Clone());
                    }

                    if (ID == 1033)
                    {
                        //// BGR
                        //for(int y=0;y<m_thumbnailImage.Height;y++)
                        //  for (int x = 0; x < m_thumbnailImage.Width; x++)
                        //  {
                        //    Color c=m_thumbnailImage.GetPixel(x,y);
                        //    Color c2=Color.FromArgb(c.B, c.G, c.R);
                        //    m_thumbnailImage.SetPixel(x, y, c);
                        //  }
                    }
                }
                else
                {
                    Image = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                }
            }
        }
    }
}