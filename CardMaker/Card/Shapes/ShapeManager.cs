////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2018 Tim Stair
//
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
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text.RegularExpressions;
using CardMaker.XML;

namespace CardMaker.Card.Shapes
{
    public class ShapeManager : IShapeRenderer
    {
        public static Dictionary<string, AbstractShape> s_dictionaryShapeByName = new Dictionary<string, AbstractShape>(); 

        // group numbers                                
        public static Regex s_regexShapes = new Regex(@"(.*)(#)(.+)(#)", RegexOptions.Compiled);

        public static Dictionary<string, AbstractShape> ShapeDictionary => s_dictionaryShapeByName;

        public static void Init()
        {
            foreach (Type typeObj in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeObj.Namespace == "CardMaker.Card.Shapes.Definitions"
                    && !typeObj.IsAbstract
                    && !typeObj.IsEnum)
                {
                    var zShape = (AbstractShape)Activator.CreateInstance(typeObj);
                    s_dictionaryShapeByName.Add(zShape.Name, zShape);
                }
            }
        }

        public void HandleShapeRender(Graphics zGraphics, string sShapeInfo, ProjectLayoutElement zElement, int nXOffset = 0, int nYOffset = 0)
        {
            if (s_regexShapes.IsMatch(sShapeInfo))
            {
                var zMatch = s_regexShapes.Match(sShapeInfo);
                ShapeInfo zInfo = null;
                var bParse = false;
                var arraySplit = zMatch.Groups[3].ToString().Split(new char[] { ';' });
                var sShapeName = arraySplit[(int)AbstractShape.ShapeInformationIndex.Name];
                AbstractShape zShape;
                if (s_dictionaryShapeByName.TryGetValue(sShapeName, out zShape))
                {
                    // allow any shapes with extended settings to read in the values (ShapeInformationIndex extension)
                    zShape.InitializeItem(sShapeInfo);

                    if ((int)AbstractShape.ShapeInformationIndex.BasicShapeInformation < arraySplit.Length)
                    {
                        int nThickness;
                        var nOverrideWidth = Int32.MinValue;
                        var nOverrideHeight = Int32.MinValue;
                        bParse = Int32.TryParse(arraySplit[(int)AbstractShape.ShapeInformationIndex.Thickness], out nThickness);
                        if (!arraySplit[(int)AbstractShape.ShapeInformationIndex.OverrideWidth].Equals(AbstractShape.NO_SIZE_OVERRIDE))
                            bParse &= Int32.TryParse(arraySplit[(int)AbstractShape.ShapeInformationIndex.OverrideWidth], out nOverrideWidth);
                        if (!arraySplit[(int)AbstractShape.ShapeInformationIndex.OverrideHeight].Equals(AbstractShape.NO_SIZE_OVERRIDE))
                            bParse &= Int32.TryParse(arraySplit[(int)AbstractShape.ShapeInformationIndex.OverrideHeight], out nOverrideHeight);
                        zInfo = new ShapeInfo(nThickness, nOverrideWidth, nOverrideHeight, arraySplit);
                    }
                    if (!bParse)
                    {
                        return; // invalid (error?)
                    }

                    var previousTransform = zGraphics.Transform;
                    var zPath = new GraphicsPath();

                    var targetRect = new Rectangle(0, 0, zElement.width - 1, zElement.height - 1);

                    // internally int.MinValue indicates no override
                    if (Int32.MinValue != zInfo.OverrideWidth || Int32.MinValue != zInfo.OverrideHeight)
                    {
                        var nOverrideWidth = Int32.MinValue == zInfo.OverrideWidth ? zElement.width : zInfo.OverrideWidth;
                        var nOverrideHeight = Int32.MinValue == zInfo.OverrideHeight ? zElement.height : zInfo.OverrideHeight;

                        if (0 == nOverrideWidth || 0 == nOverrideHeight)
                            // nothing to draw
                            return;
                        targetRect = GetZeroRectangle(nOverrideWidth, nOverrideHeight);
                        zGraphics.TranslateTransform(targetRect.X, targetRect.Y);
                        targetRect = new Rectangle(0, 0, Math.Abs(nOverrideWidth), Math.Abs(nOverrideHeight));
                    }
                    else if(nXOffset != 0 || nYOffset != 0)
                    {
                        zGraphics.TranslateTransform(nXOffset, nYOffset);
                    }

                    zShape.DrawShape(zPath, targetRect, zInfo);
                    CardRenderer.DrawPathOutline(zElement, zGraphics, zPath);

                    if (0 == zInfo.Thickness)
                    {
                        var zShapeBrush = 255 != zElement.opacity 
                            ? new SolidBrush(Color.FromArgb(zElement.opacity, zElement.GetElementColor()))
                            : new SolidBrush(zElement.GetElementColor());
                        zGraphics.FillPath(zShapeBrush, zPath);
                    }
                    else
                    {
                        var zShapePen = 255 != zElement.opacity
                            ? new Pen(Color.FromArgb(zElement.opacity, zElement.GetElementColor()), zInfo.Thickness)
                            : new Pen(zElement.GetElementColor(), zInfo.Thickness);
                        zGraphics.DrawPath(zShapePen, zPath);
                    }
                    zGraphics.Transform = previousTransform;
                }
            }
        }

        /// <summary>
        /// Creates a rectangle based on the specified width and height, if either is negative the rectangle is shifted around the origin
        /// </summary>
        /// <param name="nWidth">The width of the desired rectangle (may be negative)</param>
        /// <param name="nHeight">The height of the desired rectangle (may be negative)</param>
        /// <returns></returns>
        private static Rectangle GetZeroRectangle(int nWidth, int nHeight)
        {
            if (nWidth >= 0)
            {
                if (nHeight >= 0)
                {
                    return new Rectangle(0, 0, nWidth, nHeight);
                }
                return new Rectangle(0, nHeight, nWidth, Math.Abs(nHeight));
            }
            
            if (nHeight >= 0)
            {
                return new Rectangle(nWidth, 0, Math.Abs(nWidth), nHeight);
            }
            return new Rectangle(nWidth, nHeight, Math.Abs(nWidth), Math.Abs(nHeight));
        }
    }
}
