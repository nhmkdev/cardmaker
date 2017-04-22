License
-------
The MIT License (MIT)

Copyright (c) 2015 Tim Stair

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Current Known Mono Issues (Lightly tested in Ubuntu 12.04 LTS)
-------------------------
I installed mono using this: "sudo apt-get install mono-complete"
In OSX you can try the following: 1) Install mono 2) install XQuartz (provides Windows.Forms support)

1) If you use a text object with a non-zero outline thickness the text may draw incorrectly.
This is the only remaining issue that I had no resolution for when evaluating the executable
through MoMA (Mono Migration Analyzer). See the details below.
__________________________
void DrawText (Graphics, ProjectLayoutElement, string, Brush, Font, Color) 	
void GraphicsPath.AddString (string, FontFamily, int, Single, RectangleF, StringFormat) 	
The layoutRect and StringFormat parameters are ignored when using libgdiplus.

void Render (ProjectLayoutElement, Graphics) 	
void GraphicsPath.AddString (string, FontFamily, int, Single, PointF, StringFormat) 	
The StringFormat parameter is ignored when using libgdiplus.
__________________________

2) When using the file browser, the relative path code will determine the full path is required instead of a relative one.
You can manually adjust to a relative project path and it will work fine.