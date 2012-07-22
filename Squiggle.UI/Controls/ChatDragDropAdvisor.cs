// -------------------------------------------------------------------------------
// 
// This file is part of the FluidKit project: http://www.codeplex.com/fluidkit
// 
// Copyright (c) 2008, The FluidKit community 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this 
// list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice, this 
// list of conditions and the following disclaimer in the documentation and/or 
// other materials provided with the distribution.
// 
// * Neither the name of FluidKit nor the names of its contributors may be used to 
// endorse or promote products derived from this software without specific prior 
// written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR 
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON 
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
// -------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using FluidKit.Helpers.DragDrop;
using Squiggle.Chat;

namespace Squiggle.UI.Controls
{
    class ChatDragDropAdvisor : IDragSourceAdvisor, IDropTargetAdvisor
    {
        static string dataObjectName = "DraggedContact_" + Guid.NewGuid();
        UIElement sourceElement;
        UIElement targetElement;

        #region IDragSourceAdvisor Members

        public UIElement SourceUI
        {
            get { return sourceElement; }
            set { sourceElement = value; }
        }

        public DragDropEffects SupportedEffects
        {
            get { return DragDropEffects.Copy | DragDropEffects.Move; }
        }

        public DataObject GetDataObject(UIElement draggedElt)
        {
            Border contact = GetContact(draggedElt);
            DataObject obj = new DataObject(dataObjectName, contact);
            return obj;
        }

        public void FinishDrag(UIElement draggedElt, DragDropEffects finalEffects)
        {
        }

        public bool IsDraggable(UIElement dragElt)
        {
            bool isContactOrChildOfIt = GetContact(dragElt) != null;
            return isContactOrChildOfIt;
        }        

        public UIElement GetTopContainer()
        {
            UIElement container = null;
            if (TargetUI != null)
            {
                var window = TargetUI.GetVisualParent<Window>();
                if (window != null)
                    container = (UIElement)window.Content;
            }

            if (container == null)
                container =  (UIElement)Application.Current.MainWindow.Content;

            return container;
        }

        #endregion

        #region IDropTargetAdvisor Members

        public UIElement TargetUI
        {
            get { return targetElement; }
            set { targetElement = value; }
        }

        public bool ApplyMouseOffset
        {
            get { return false; }
        }

        public bool IsValidDataObject(IDataObject obj)
        {
            bool isValid = obj.GetDataPresent(DataFormats.FileDrop) || obj.GetDataPresent(dataObjectName);
            return isValid;
        }

        public UIElement GetVisualFeedback(IDataObject obj)
        {
            var contact = GetContact(obj);

            if (contact != null)
            {
                Rectangle rect = new Rectangle();
                rect.Width = contact.ActualWidth;
                rect.Height = contact.ActualHeight;
                rect.Fill = new VisualBrush(contact);
                rect.Opacity = 0.5;
                rect.IsHitTestVisible = false;

                return rect;
            }
            else
                return null;
        }

        public void OnDropCompleted(IDataObject obj, Point dropPoint)
        {
            var chatWindow = TargetUI.GetVisualParent<ChatWindow>();
            if (chatWindow != null)
            {
                if (obj.GetDataPresent(DataFormats.FileDrop))
                    chatWindow.SendFiles(obj.GetData(DataFormats.FileDrop) as string[]);
                else
                {
                    var item = GetContact(obj);
                    var buddy = item.DataContext as Buddy;
                    chatWindow.Invite(buddy);
                }                
            }
        }

        #endregion

        static Border GetContact(UIElement element)
        {
            if (IsContact(element as FrameworkElement))
                return element as Border;
            return element.GetVisualParent<Border>(border => IsContact(border));
        }

        static bool IsContact(FrameworkElement element)
        {
            return element is Border && element.Name == "contactBorder";
        }

        static Border GetContact(IDataObject obj)
        {
            var contact = obj.GetData(dataObjectName) as Border;
            return contact;
        }
    }
}
