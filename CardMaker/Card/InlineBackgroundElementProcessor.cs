////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2020 Tim Stair
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
using System.Text.RegularExpressions;
using CardMaker.Card.Shapes;
using CardMaker.Data;
using CardMaker.XML;
using Support.Util;

namespace CardMaker.Card
{
    /// <summary>
    /// This is a brutal collection of regexes intended for inline background images/shapes
    /// The lists are in order of preference (basically most params to least params)
    /// </summary>
    public class InlineBackgroundElementProcessor : IInlineBackgroundElementProcessor
    {
        private List<KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>> listGraphicProcessingPairs =
            new List<KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>>()
            {
                // extended
                // #bggraphic:[image path]:[x offset]:[y offset]:[width adjust]:[height adjust]:[lock aspect ratio]:[tile size]:[horizontal align]:[vertical align]#
                new KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>(
                    //           1             2    3   4    5   6    7   8    9   10   11  12   13  14   15  16   17  18   19
                    new Regex(@"(#bggraphic::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(#)", RegexOptions.Compiled),
                    (zMatch, zBgGraphicElement, zElement, pointOffset) =>
                    {
                        pointOffset.X = ParseUtil.ParseDefault(zMatch.Groups[4].Value, 0);
                        pointOffset.Y = ParseUtil.ParseDefault(zMatch.Groups[6].Value, 0);
                        zBgGraphicElement.width = zElement.width + ParseUtil.ParseDefault(zMatch.Groups[8].Value, 0);
                        zBgGraphicElement.height = zElement.height + ParseUtil.ParseDefault(zMatch.Groups[10].Value, 0);
                        zBgGraphicElement.opacity = zElement.opacity;
                        zBgGraphicElement.lockaspect = ParseUtil.ParseDefault(zMatch.Groups[12].Value, false);
                        zBgGraphicElement.tilesize = zMatch.Groups[14].Value;
                        zBgGraphicElement.horizontalalign = ParseUtil.ParseDefault(zMatch.Groups[16].Value, 0);
                        zBgGraphicElement.verticalalign = ParseUtil.ParseDefault(zMatch.Groups[18].Value, 0);
                        zBgGraphicElement.variable = zMatch.Groups[2].Value;
                        zBgGraphicElement.type = ElementType.Graphic.ToString();
                    }),
                // simple
                new KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>(
                    //           1             2    3
                    new Regex(@"(#bggraphic::)(.+?)(#)", RegexOptions.Compiled),
                    (zMatch, zBgGraphicElement, zElement, pointOffset) =>
                    {
                        zBgGraphicElement.width = zElement.width;
                        zBgGraphicElement.height = zElement.height;
                        zBgGraphicElement.opacity = zElement.opacity;
                        zBgGraphicElement.variable = zMatch.Groups[2].Value;
                        zBgGraphicElement.type = ElementType.Graphic.ToString();
                    })
            };


        private List<KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>> listShapeProcessingPairs =
            new List<KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>>()
            {
                // extended + opacity
                new KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>(
                    //           1           2    3   4    5   6    7   8    9   10   11  12   13  14   15  16   17  18   19
                    new Regex(@"(#bgshape::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(#)", RegexOptions.Compiled),
                    (zMatch, zBgShapeElement, zElement, pointOffset) =>
                    {
                        pointOffset.X = ParseUtil.ParseDefault(zMatch.Groups[6].Value, 0);
                        pointOffset.Y = ParseUtil.ParseDefault(zMatch.Groups[8].Value, 0);
                        zBgShapeElement.x = zElement.x;
                        zBgShapeElement.y = zElement.y;
                        zBgShapeElement.width =
                            zElement.width + ParseUtil.ParseDefault(zMatch.Groups[10].Value, 0);
                        zBgShapeElement.height =
                            zElement.height + ParseUtil.ParseDefault(zMatch.Groups[12].Value, 0);
                        zBgShapeElement.outlinethickness = ParseUtil.ParseDefault(zMatch.Groups[14].Value, 0);
                        zBgShapeElement.outlinecolor = zMatch.Groups[16].Value;
                        zBgShapeElement.elementcolor = zMatch.Groups[4].Value;
                        zBgShapeElement.opacity = ParseUtil.ParseDefault(zMatch.Groups[18].Value, 255);
                        zBgShapeElement.variable = zMatch.Groups[2].Value;
                        zBgShapeElement.type = ElementType.Shape.ToString();
                    }),
                // extended
                new KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>(
                    //           1           2    3   4    5   6    7   8    9   10   11  12   13  14   15  16   17
                    new Regex(@"(#bgshape::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(::)(.+?)(#)", RegexOptions.Compiled),
                    (zMatch, zBgShapeElement, zElement, pointOffset) =>
                    {
                        pointOffset.X = ParseUtil.ParseDefault(zMatch.Groups[6].Value, 0);
                        pointOffset.Y = ParseUtil.ParseDefault(zMatch.Groups[8].Value, 0);
                        zBgShapeElement.x = zElement.x;
                        zBgShapeElement.y = zElement.y;
                        zBgShapeElement.width = zElement.width + ParseUtil.ParseDefault(zMatch.Groups[10].Value, 0);
                        zBgShapeElement.height = zElement.height + ParseUtil.ParseDefault(zMatch.Groups[12].Value, 0);
                        zBgShapeElement.outlinethickness = ParseUtil.ParseDefault(zMatch.Groups[14].Value, 0);
                        zBgShapeElement.outlinecolor = zMatch.Groups[16].Value;
                        zBgShapeElement.elementcolor = zMatch.Groups[4].Value;
                        zBgShapeElement.variable = zMatch.Groups[2].Value;
                        zBgShapeElement.type = ElementType.Shape.ToString();
                    }),
                // simple
                new KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>(
                    //           1           2    3   4    5
                    new Regex(@"(#bgshape::)(.+?)(::)(.+?)(#)", RegexOptions.Compiled),
                    (zMatch, zBgShapeElement, zElement, pointOffset) =>
                    {
                        zBgShapeElement.x = zElement.x;
                        zBgShapeElement.y = zElement.y;
                        zBgShapeElement.width = zElement.width;
                        zBgShapeElement.height = zElement.height;
                        zBgShapeElement.elementcolor = zMatch.Groups[4].Value;
                        zBgShapeElement.variable = zMatch.Groups[2].Value;
                        zBgShapeElement.type = ElementType.Shape.ToString();
                    })
            };

        public string ProcessInlineBackgroundGraphic(IDrawGraphic zDrawGraphic, Graphics zGraphics, ProjectLayoutElement zElement, string sInput)
        {
            var kvpMatchedProcessor = new KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>();
            Match zMatch = null;
            foreach (var kvpGraphicProcessor in listGraphicProcessingPairs)
            {
                zMatch = kvpGraphicProcessor.Key.Match(sInput);
                if (zMatch.Success)
                {
                    kvpMatchedProcessor = kvpGraphicProcessor;
                    break;
                }
            }

            if (kvpMatchedProcessor.Key == null
                || zMatch == null
                || !zMatch.Success)
                return sInput;

            var sToReplace = zMatch.Groups[0].Value;
            var pointOffset = new PointOffset();
            var zBgGraphicElement = new ProjectLayoutElement(Guid.NewGuid().ToString());
            zBgGraphicElement.opacity = -1;

            kvpMatchedProcessor.Value(zMatch, zBgGraphicElement, zElement, pointOffset);

            zDrawGraphic.DrawGraphicFile(zGraphics, zBgGraphicElement.variable, zBgGraphicElement, pointOffset.X, pointOffset.Y);

            return sInput.Replace(sToReplace, string.Empty);
        }

        public string ProcessInlineShape(IShapeRenderer zShapeRenderer, Graphics zGraphics, ProjectLayoutElement zElement, string sInput)
        {
            var kvpMatchedProcessor = new KeyValuePair<Regex, Action<Match, ProjectLayoutElement, ProjectLayoutElement, PointOffset>>();
            Match zMatch = null;
            foreach (var kvpShapeProcessor in listShapeProcessingPairs)
            {
                zMatch = kvpShapeProcessor.Key.Match(sInput);
                if (zMatch.Success)
                {
                    kvpMatchedProcessor = kvpShapeProcessor;
                    break;
                }
            }

            if (kvpMatchedProcessor.Key == null
                || zMatch == null 
                || !zMatch.Success)
                return sInput;

            var sToReplace = zMatch.Groups[0].Value;
            var pointOffset = new PointOffset();
            var zBgShapeElement = new ProjectLayoutElement(Guid.NewGuid().ToString());
            zBgShapeElement.opacity = -1;

            kvpMatchedProcessor.Value(zMatch, zBgShapeElement, zElement, pointOffset);

            zBgShapeElement.InitializeTranslatedFields();

            // the processor method didn't tweak the opacity, default to the element color alpha channel
            if(zBgShapeElement.opacity == -1)
                zBgShapeElement.opacity = zBgShapeElement.GetElementColor().A;

            zShapeRenderer.HandleShapeRender(zGraphics, zBgShapeElement.variable, zBgShapeElement, pointOffset.X, pointOffset.Y);

            return sInput.Replace(sToReplace, string.Empty);
        }
    }

    class PointOffset
    {
        public int X { get; set; }
        public int Y { get; set; }
        public PointOffset()
        { }
        public PointOffset(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
