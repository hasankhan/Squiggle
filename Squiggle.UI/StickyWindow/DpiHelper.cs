/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Squiggle.UI.StickyWindow
{
    internal static class DpiHelper
    {
        private static Matrix _transformToDevice;
        private static Matrix _transformToDip;

        [
            SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations"),
            SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")
        ]
        static DpiHelper()
        {
            // Getting the Desktop, so we shouldn't have to release this DC.
            // This could potentially throw if used in a service.
            IntPtr desktop = NativeMethods.GetDC(IntPtr.Zero);

            // Can get these in the static constructor.  They shouldn't vary window to window,
            // and changing the system DPI requires a restart.
            int pixelsPerInchX = NativeMethods.GetDeviceCaps(desktop, NativeMethods.DeviceCap.LOGPIXELSX);
            int pixelsPerInchY = NativeMethods.GetDeviceCaps(desktop, NativeMethods.DeviceCap.LOGPIXELSY);

            _transformToDip = Matrix.Identity;
            _transformToDip.Scale(96d / (double)pixelsPerInchX, 96d / (double)pixelsPerInchY);
            _transformToDevice = Matrix.Identity;
            _transformToDevice.Scale((double)pixelsPerInchX / 96d, (double)pixelsPerInchY / 96d);
        }

        /// <summary>
        /// Convert a point in device independent pixels (1/96") to a point in the system coordinates.
        /// </summary>
        /// <param name="logicalPoint">A point in the logical coordinate system.</param>
        /// <returns>Returns the parameter converted to the system's coordinates.</returns>
        public static Point LogicalPixelsToDevice(Point logicalPoint)
        {
            return _transformToDevice.Transform(logicalPoint);
        }

        public static Thickness LogicalPixelsToDevice(Thickness logicalThickness)
        {
            Point deviceTopLeft = _transformToDevice.Transform(new Point(logicalThickness.Left, logicalThickness.Top));
            Point deviceBottomRight = _transformToDevice.Transform(new Point(logicalThickness.Right, logicalThickness.Bottom));

            return new Thickness(deviceTopLeft.X, deviceTopLeft.Y, deviceBottomRight.X, deviceBottomRight.Y);
        }


        /// <summary>
        /// Convert a point in system coordinates to a point in device independent pixels (1/96").
        /// </summary>
        /// <param name="devicePoint">A point in the physical coordinate system.</param>
        /// <returns>Returns the parameter converted to the device independent coordinate system.</returns>
        public static Point DevicePixelsToLogical(Point devicePoint)
        {
            return _transformToDip.Transform(devicePoint);
        }

        /// <summary>
        /// Scale a rectangle in system coordinates to a rectangle in device independent pixels (1/96").
        /// </summary>
        /// <param name="deviceRectangle">A rectangle in the physical coordinate system.</param>
        /// <returns>Returns the parameter converted to the device independent coordinate system.</returns>
        public static Rect DeviceRectToLogical(Rect deviceRectangle)
        {
            Point topLeft = DevicePixelsToLogical(new Point(deviceRectangle.Left, deviceRectangle.Top));
            Point bottomRight = DevicePixelsToLogical(new Point(deviceRectangle.Right, deviceRectangle.Bottom));

            return new Rect(topLeft, bottomRight);
        }
    }
}