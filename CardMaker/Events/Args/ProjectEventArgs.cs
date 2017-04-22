﻿////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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

using CardMaker.XML;

namespace CardMaker.Events.Args
{
    public delegate void ProjectOpened(object sender, ProjectEventArgs e);

    public delegate void ProjectUpdated(object sender, ProjectEventArgs e);

    public class ProjectEventArgs
    {
        public Project Project { get; private set; }
        public string ProjectFilePath { get; private set; }
        public bool DataChange { get; private set; }
        
        public ProjectEventArgs(Project zProject, string sProjectFilePath, bool bDataChange)
        {
            Project = zProject;
            ProjectFilePath = sProjectFilePath;
            DataChange = bDataChange;
        }

        public ProjectEventArgs(Project zProject, string sProjectFilePath)
            : this(zProject, sProjectFilePath, false)
        {
        }
    }
}
