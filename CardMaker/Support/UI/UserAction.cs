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

namespace Support.UI
{
    /// <summary>
    /// Class for managing the undo and redo stacks
    /// </summary>
    class UserAction
    {
        private static readonly Stack<Action<bool>> m_stackUndo = new Stack<Action<bool>>(500);
        private static readonly Stack<Action<bool>> m_stackRedo = new Stack<Action<bool>>(500);

        public static Action OnClearUserActions { get; set; }

        /// <summary>
        /// Pushes an action onto the action stack
        /// </summary>
        /// <param name="zAction">The action to push</param>
        /// <param name="bPerformAction">Flag indicating whether to execute the action after pushing it</param>
        public static void PushAction(Action<bool> zAction, bool bPerformAction = false)
        {
            m_stackUndo.Push(zAction);
            if (bPerformAction)
            {
                zAction(true);
            }

            // An external change must erase the entire redo stack, 
            // since the redo operations will no longer be consistent with the current state!
            m_stackRedo.Clear();
        }

        /// <summary>
        /// Pops the undo action and pushes it to the redo stack. The action is not executed.
        /// </summary>
        /// <returns></returns>
        public static Action<bool> GetUndoAction()
        {
            // pop the undo stack and push into the redo
            Action<bool> zAction = null;
            if (m_stackUndo.Count > 0)
            {
                zAction = m_stackUndo.Pop();
                m_stackRedo.Push(zAction);
            }
            return zAction;
        }

        /// <summary>
        /// Pops the redo action and pushes it to the redo stack. The action is not executed.
        /// </summary>
        /// <returns></returns>
        public static Action<bool> GetRedoAction()
        {
            // pop the redo stack and push into the undo
            Action<bool> zAction = null;
            if (m_stackRedo.Count > 0)
            {
                zAction = m_stackRedo.Pop();
                m_stackUndo.Push(zAction);
            }
            return zAction;
        }

        public static List<Action<bool>> CreateActionList()
        {
            return new List<Action<bool>>();
        }

        public static void ClearUndoRedoStacks()
        {
            m_stackRedo.Clear();
            m_stackUndo.Clear();
            OnClearUserActions?.Invoke();
        }

        public static int UndoCount => m_stackUndo.Count;
        public static int RedoCount => m_stackRedo.Count;
    }

}