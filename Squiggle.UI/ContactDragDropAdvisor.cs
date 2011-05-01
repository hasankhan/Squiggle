using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidKit.Helpers.DragDrop;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Xml;
using System.IO;

namespace Squiggle.UI
{
    class ContactDragDropAdvisor : IDragSourceAdvisor, IDropTargetAdvisor
    {
        bool _applyMouseOffset;
        UIElement _sourceAndTargetElt;

        #region IDragSourceAdvisor Members

        public UIElement SourceUI
        {
            get { return _sourceAndTargetElt; }
            set { _sourceAndTargetElt = value; }
        }

        public DragDropEffects SupportedEffects
        {
            get { return DragDropEffects.Copy | DragDropEffects.Move; }
        }

        public DataObject GetDataObject(UIElement draggedElt)
        {
            string serializedElt = XamlWriter.Save(draggedElt);
            DataObject obj = new DataObject("CanvasExample", serializedElt);

            return obj;
        }

        public void FinishDrag(UIElement draggedElt, DragDropEffects finalEffects)
        {
            if ((finalEffects & DragDropEffects.Move) == DragDropEffects.Move)
                (_sourceAndTargetElt as Canvas).Children.Remove(draggedElt);
        }

        public bool IsDraggable(UIElement dragElt)
        {
            return !(dragElt is Canvas);
        }

        public UIElement GetTopContainer()
        {
            return Application.Current.MainWindow.Content as UIElement;
        }

        #endregion

        #region IDropTargetAdvisor Members

        public UIElement TargetUI
        {
            get { return _sourceAndTargetElt; }
            set { _sourceAndTargetElt = value; }
        }

        public bool ApplyMouseOffset
        {
            get { return _applyMouseOffset; }
        }

        public bool IsValidDataObject(IDataObject obj)
        {
            return (obj.GetDataPresent("CanvasExample"));
        }

        public UIElement GetVisualFeedback(IDataObject obj)
        {
            UIElement elt = ExtractElement(obj);

            Type t = elt.GetType();

            Rectangle rect = new Rectangle();
            rect.Width = (double)t.GetProperty("Width").GetValue(elt, null);
            rect.Height = (double)t.GetProperty("Height").GetValue(elt, null);
            rect.Fill = new VisualBrush(elt);
            rect.Opacity = 0.5;
            rect.IsHitTestVisible = false;

            return rect;
        }

        public void OnDropCompleted(IDataObject obj, Point dropPoint)
        {
            Canvas canvas = _sourceAndTargetElt as Canvas;

            UIElement elt = ExtractElement(obj);
            canvas.Children.Add(elt);
            Canvas.SetLeft(elt, dropPoint.X);
            Canvas.SetTop(elt, dropPoint.Y);
        }

        #endregion

        private UIElement ExtractElement(IDataObject obj)
        {
            string xamlString = obj.GetData("CanvasExample") as string;
            XmlReader reader = XmlReader.Create(new StringReader(xamlString));
            UIElement elt = XamlReader.Load(reader) as UIElement;

            return elt;
        }
    }
}
