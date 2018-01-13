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
using System.Drawing;
using System.Text.RegularExpressions;
using CardMaker.Card.Shapes;
using CardMaker.Data;
using CardMaker.XML;
using Support.Util;

namespace CardMaker.Card
{
    public class InlineBackgroundElementProcessor : IInlineBackgroundElementProcessor
    {
        //                                                        1          2    3
        private static readonly Regex regexGraphicBG = new Regex(@"(#bggraphic::)(.+?)(#)", RegexOptions.Compiled);
        //                        #bggraphic:[image path]:[x offset]:[y offset]:[width adjust]:[height adjust]:[lock aspect ratio]:[tile size]:[horizontal align]:[vertical align]#
        //                                                                1          2    3  4    5  6    7  8    9  10   11 12   13 14   15 16      17
        private static readonly Regex regexGraphicExtendedBG = new Regex(@"(#bggraphic::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(#)", RegexOptions.Compiled);

        //                                                        1          2    3  4    5
        private static readonly Regex regexShapeBG = new Regex(@"(#bgshape::)(.+?)(::)(.+?)(#)", RegexOptions.Compiled);

        //                                                                1          2    3  4    5  6    7  8    9  10   11 12   13 14   15 16   17
        private static readonly Regex regexShapeExtendedBG = new Regex(@"(#bgshape::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(#)", RegexOptions.Compiled);

        public string ProcessInlineBackgroundGraphic(IDrawGraphic zDrawGraphic, Graphics zGraphics, ProjectLayoutElement zElement, string sInput)
        {
            var zExtendedMatch = regexGraphicExtendedBG.Match(sInput);
            Match zMatch = null;
            if (!zExtendedMatch.Success)
            {
                zMatch = regexGraphicBG.Match(sInput);
                if (!zMatch.Success)
                {
                    return sInput;
                }
            }

            var sToReplace = String.Empty;

            int[] arrayReplaceIndcies = null;
            var zBgGraphicElement = new ProjectLayoutElement(Guid.NewGuid().ToString());
            var nXOffset = 0;
            var nYOffset = 0;
            if (zExtendedMatch.Success)
            {
                nXOffset = ParseUtil.ParseDefault(zExtendedMatch.Groups[4].Value, 0);
                nYOffset = ParseUtil.ParseDefault(zExtendedMatch.Groups[6].Value, 0);
                zBgGraphicElement.width = zElement.width + ParseUtil.ParseDefault(zExtendedMatch.Groups[8].Value, 0);
                zBgGraphicElement.height = zElement.height + ParseUtil.ParseDefault(zExtendedMatch.Groups[10].Value, 0);
                zBgGraphicElement.opacity = zElement.opacity;
                zBgGraphicElement.lockaspect = ParseUtil.ParseDefault(zExtendedMatch.Groups[12].Value, false);
                zBgGraphicElement.tilesize = zExtendedMatch.Groups[14].Value;
                zBgGraphicElement.horizontalalign = ParseUtil.ParseDefault(zExtendedMatch.Groups[16].Value, 0);
                zBgGraphicElement.verticalalign = ParseUtil.ParseDefault(zExtendedMatch.Groups[18].Value, 0);
                zBgGraphicElement.variable = zExtendedMatch.Groups[2].Value;
                zBgGraphicElement.type = ElementType.Graphic.ToString();
                sToReplace = zExtendedMatch.Groups[0].Value;
            }
            else if (zMatch.Success)
            {
                zBgGraphicElement.width = zElement.width;
                zBgGraphicElement.height = zElement.height;
                zBgGraphicElement.opacity = zElement.opacity;
                zBgGraphicElement.variable = zMatch.Groups[2].Value;
                zBgGraphicElement.type = ElementType.Graphic.ToString();
                sToReplace = zMatch.Groups[0].Value;
            }

            zDrawGraphic.DrawGraphicFile(zGraphics, zBgGraphicElement.variable, zBgGraphicElement, nXOffset, nYOffset);

            return sInput.Replace(sToReplace, string.Empty);
        }

        public string ProcessInlineShape(IShapeRenderer zShapeRenderer, Graphics zGraphics, ProjectLayoutElement zElement, string sInput)
        {
            var zExtendedMatch = regexShapeExtendedBG.Match(sInput);
            Match zMatch = null;
            if (!zExtendedMatch.Success)
            {
                zMatch = regexShapeBG.Match(sInput);
                if (!zMatch.Success)
                {
                    return sInput;
                }
            }

            var sToReplace = String.Empty;
            int nXOffset = 0, nYOffset = 0;

            int[] arrayReplaceIndcies = null;
            var zBgShapeElement = new ProjectLayoutElement(Guid.NewGuid().ToString());
            if (zExtendedMatch.Success)
            {
                nXOffset = ParseUtil.ParseDefault(zExtendedMatch.Groups[6].Value, 0);
                nYOffset = ParseUtil.ParseDefault(zExtendedMatch.Groups[8].Value, 0);
                zBgShapeElement.x = zElement.x;
                zBgShapeElement.y = zElement.y;
                zBgShapeElement.width = zElement.width + ParseUtil.ParseDefault(zExtendedMatch.Groups[10].Value, 0);
                zBgShapeElement.height = zElement.height + ParseUtil.ParseDefault(zExtendedMatch.Groups[12].Value, 0);
                zBgShapeElement.outlinethickness = ParseUtil.ParseDefault(zExtendedMatch.Groups[14].Value, 0);
                zBgShapeElement.outlinecolor = zExtendedMatch.Groups[16].Value;
                zBgShapeElement.elementcolor = zExtendedMatch.Groups[4].Value;
                zBgShapeElement.variable = zExtendedMatch.Groups[2].Value;
                zBgShapeElement.type = ElementType.Shape.ToString();
                sToReplace = zExtendedMatch.Groups[0].Value;
            }
            else if (zMatch.Success)
            {
                zBgShapeElement.x = zElement.x;
                zBgShapeElement.y = zElement.y;
                zBgShapeElement.width = zElement.width;
                zBgShapeElement.height = zElement.height;
                zBgShapeElement.elementcolor = zMatch.Groups[4].Value;
                zBgShapeElement.variable = zMatch.Groups[2].Value;
                zBgShapeElement.type = ElementType.Shape.ToString();
                sToReplace = zMatch.Groups[0].Value;
            }

            zBgShapeElement.InitializeTranslatedFields();

            zShapeRenderer.HandleShapeRender(zGraphics, zBgShapeElement.variable, zBgShapeElement, nXOffset, nYOffset);

            return sInput.Replace(sToReplace, String.Empty);
        }
    }
}
