////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
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

using Support.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Support.UI
{
    class TreeViewEx : System.Windows.Forms.TreeView
    {
        private HashSet<TreeNode> m_setSelectedNodes = new HashSet<TreeNode>();
        private List<TreeNode> m_listSelectedNodes = new List<TreeNode>();

        public TreeViewEx() : base()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            DrawNode += TreeViewEx_DrawNode;
            NodeMouseClick += TreeViewEx_NodeMouseClick;
            AllowDrop = true;

            // This stuff may be too specific to CardMaker. May want to separate.
            ItemDrag += TreeViewEx_ItemDrag;
            DragDrop += TreeViewEx_DragDrop;
            DragOver += TreeViewEx_DragOver;
            DragEnter += TreeViewEx_DragEnter;

        }

        private void TreeViewEx_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void TreeViewEx_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Move the dragged node when the left mouse button is used.
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }

            // Copy the dragged node when the right mouse button is used.
            else if (e.Button == MouseButtons.Right)
            {
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }

        private void TreeViewEx_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.
            Point targetPoint = PointToClient(new Point(e.X, e.Y));

            // Select the node at the mouse position.
            SelectedNode = GetNodeAt(targetPoint);
        }

        private void TreeViewEx_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            TreeNode targetNode = GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Confirm that the node at the drop location is not 
            // the dragged node or a descendant of the dragged node.
            if (!draggedNode.Equals(targetNode) && !ContainsNode(draggedNode, targetNode))
            {
                // If it is a move operation, remove the node from its current 
                // location and add it to the node at the drop location.
                if (e.Effect == DragDropEffects.Move)
                {
                    draggedNode.Remove();
                    targetNode.Nodes.Add(draggedNode);
                }

                // If it is a copy operation, clone the dragged node 
                // and add it to the node at the drop location.
                else if (e.Effect == DragDropEffects.Copy)
                {
                    targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                }

                // Expand the node at the location 
                // to show the dropped node.
                targetNode.Expand();
            }
        }

        // Determine whether one node is a parent 
        // or ancestor of a second node.
        private bool ContainsNode(TreeNode node1, TreeNode node2)
        {
            // Check the parent node of the second node.
            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            // If the parent node is not null or equal to the first node, 
            // call the ContainsNode method recursively using the parent of 
            // the second node.
            return ContainsNode(node1, node2.Parent);
        }

        private bool IsNodeSelected(TreeNode zNode)
        {
            return m_setSelectedNodes.Contains(zNode);
        }

        private void SetSelectedNodes(List<TreeNode> listSelectedNodes)
        {
            m_setSelectedNodes.Clear();
            m_listSelectedNodes.Clear();
            if (listSelectedNodes == null)
            {
                Invalidate();
            }
            if (listSelectedNodes != null)
            {
                listSelectedNodes.ForEach(node => m_setSelectedNodes.Add(node));
                m_listSelectedNodes.AddRange(listSelectedNodes);
            }
        }

        private void ToggleSelectedNode(TreeNode zNode)
        {
            if (m_setSelectedNodes.Contains(zNode))
            {
                m_setSelectedNodes.Remove(zNode);
                m_listSelectedNodes.Remove(zNode);
            }
            else
            {
                m_setSelectedNodes.Add(zNode);
                m_listSelectedNodes.Add(zNode);
            }
        }

        private void TreeViewEx_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var hitTest = e.Node.TreeView.HitTest(e.Location);
            if (hitTest.Location == TreeViewHitTestLocations.PlusMinus)
                return;

            Logger.AddLogLine("Selected Node Index: {0}".FormatString(e.Node.Index));
            if (ModifierKeys == Keys.Control)
            {
                ToggleSelectedNode(e.Node);
            }
            else
            {
                // reset all selected nodes (new selection)
                SetSelectedNodes(null);
                ToggleSelectedNode(e.Node);
            }
        }

        private void TreeViewEx_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var bIsSelected = IsNodeSelected(e.Node);
            var zBackBrush = bIsSelected 
                ? new SolidBrush(SystemColors.Highlight) 
                : new SolidBrush(e.Node.TreeView.BackColor);
            var zForeBrush = bIsSelected 
                ? new SolidBrush(SystemColors.HighlightText) 
                : new SolidBrush(e.Node.TreeView.ForeColor);
            e.Graphics.FillRectangle(zBackBrush, e.Node.Bounds);
            var zFont = e.Node.NodeFont ?? e.Node.TreeView.Font;
            e.Graphics.DrawString(e.Node.Text, zFont, zForeBrush, e.Bounds);
            // TODO: cache fonts and brushes
            // if (e.Node.Checked) nodeFont.Dispose();
        }
    }
}
