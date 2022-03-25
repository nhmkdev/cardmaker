////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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

using System.Collections.Generic;
using CardMaker.XML;
using Support.Progress;

namespace CardMaker.Card.Import
{
    public abstract class ReferenceReader
    {
        public string ReferencePath { get; protected set; }
        public IProgressReporter ProgressReporter { get; set; }

        /// <summary>
        /// Reads the reference data into the specified list
        /// </summary>
        /// <param name="zReference">The reference meta data</param>
        /// <param name="listReferenceData">The list to append</param>
        public abstract void GetReferenceData(ProjectLayoutReference zReference, List<List<string>> listReferenceData);
        /// <summary>
        /// Reads the project define data into the specified list
        /// </summary>
        /// <param name="zReference">The reference meta data</param>
        /// <param name="listReferenceData">The list to append</param>
        public abstract void GetProjectDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData);
        /// <summary>
        /// Reads the reference define data into the specified list
        /// </summary>
        /// <param name="zReference">The reference meta data</param>
        /// <param name="listReferenceData">The list to append</param>
        public abstract void GetDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData);

        /// <summary>
        /// Post constructor initialization
        /// </summary>
        public virtual ReferenceReader Initialize()
        {
            return this;
        }

        /// <summary>
        /// Called to signify that all references have been loaded
        /// </summary>
        public virtual void FinalizeReferenceLoad()
        {

        }

        /// <summary>
        /// Used to indicate the reader is in a valid state
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// Used to handle an invalid reference reader
        /// </summary>
        public virtual void HandleInvalid()
        {

        }
    }
}
