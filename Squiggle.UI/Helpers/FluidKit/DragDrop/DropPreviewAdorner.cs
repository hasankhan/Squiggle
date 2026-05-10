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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FluidKit.Helpers.DragDrop
{
	public class DropPreviewAdorner : Adorner
	{
		private double _left;
		private ContentPresenter _presenter;
		private double _top;

		public DropPreviewAdorner(UIElement feedbackUI, UIElement adornedElt) : base(adornedElt)
		{
			_presenter = new ContentPresenter();
			_presenter.Content = feedbackUI;
			_presenter.IsHitTestVisible = false;
		}

		public double Left
		{
			get { return _left; }
			set
			{
				_left = value;
				UpdatePosition();
			}
		}

		public double Top
		{
			get { return _top; }
			set
			{
				_top = value;
				UpdatePosition();
			}
		}

		protected override int VisualChildrenCount
		{
			get { return 1; }
		}

		private void UpdatePosition()
		{
			AdornerLayer layer = this.Parent as AdornerLayer;
			if (layer != null)
			{
				layer.Update(AdornedElement);
			}
		}

		protected override Size MeasureOverride(Size constraint)
		{
			_presenter.Measure(constraint);
			return _presenter.DesiredSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			_presenter.Arrange(new Rect(finalSize));
			return finalSize;
		}

		protected override Visual GetVisualChild(int index)
		{
			return _presenter;
		}

		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{
			GeneralTransformGroup result = new GeneralTransformGroup();
			result.Children.Add(new TranslateTransform(Left, Top));
			if (Left > 0) this.Visibility = Visibility.Visible;
			result.Children.Add(base.GetDesiredTransform(transform));

			return result;
		}
	}
}
