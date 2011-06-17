using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Squiggle.Utilities;
using System.Threading;
using System.Collections.Generic;

namespace Squiggle.UI
{
    public static class WPFExtensions
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        public static void MoveToBottom(this Window window)
        {
            var interopHelper = new WindowInteropHelper(window);
            SetWindowPos(interopHelper.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        public static TParent GetVisualParent<TParent>(this DependencyObject element) where TParent:DependencyObject
        {
            return GetVisualParent<TParent>(element, _ => true);
        }

        public static TParent GetVisualParent<TParent>(this DependencyObject element, Predicate<TParent> filter) where TParent:DependencyObject
        {
            do
            {
                element = VisualTreeHelper.GetParent(element);
            }
            while (element != null && (!(element is TParent) || !filter((TParent)element)));
            return (TParent)element;
        }
        
        public static IEnumerable<TChild> GetVisualChildren<TChild>(this DependencyObject element) where TChild:DependencyObject
        {
            return GetVisualChildren<TChild>(element, _ => true);
        }

        public static IEnumerable<TChild> GetVisualChildren<TChild>(this DependencyObject element, Predicate<TChild> filter) where TChild:DependencyObject
        {
            var pending = new Queue<DependencyObject>();
            pending.Enqueue(element);

            while (pending.Count > 0)
            {
                DependencyObject parent = pending.Dequeue();
                int count = VisualTreeHelper.GetChildrenCount(parent);
                if (count > 0)
                    for (int i = 0; i < count; i++)
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                        if (child is TChild && filter((TChild)child))
                            yield return (TChild)child;
                        else
                            pending.Enqueue(child);
                    }
            }
        }

        public static ScrollViewer FindScrollViewer(this FlowDocumentScrollViewer flowDocumentScrollViewer)
        {
            if (VisualTreeHelper.GetChildrenCount(flowDocumentScrollViewer) == 0)
            {
                return null;
            }

            // Border is the first child of first child of a ScrolldocumentViewer 
            DependencyObject firstChild = VisualTreeHelper.GetChild(flowDocumentScrollViewer, 0);
            if (firstChild == null)
            {
                return null;
            }

            Decorator border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;

            if (border == null)
            {
                return null;
            }

            return border.Child as ScrollViewer;
        }        
    }
}
