/*****************************************************
 * 
 * ReoGrid - .NET Spreadsheet Control
 * 
 * https://reogrid.net/
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 *
 * Author: Jingwood <jingwood at unvell.com>
 *
 * Copyright (c) 2012-2025 Jingwood <jingwood at unvell.com>
 * Copyright (c) 2012-2025 UNVELL Inc. All rights reserved.
 * 
 ****************************************************************************/using System;



namespace Calcita.Graphics
{
    /// <summary>
    /// Represents point information that includes the x-coordinate value and y-coordinate value.
    /// </summary>
    [Serializable]
    public struct Point
    {
        /// <summary>
        /// Get or set the value on x-coordinate.
        /// </summary>
        public RGFloat X { get; set; }

        /// <summary>
        /// Get or set the value on y-coordinate.
        /// </summary>
        public RGFloat Y { get; set; }

        /// <summary>
        /// Create point by specified x-coordinate value and y-coordinate value.
        /// </summary>
        /// <param name="x">Value on x-coordinate.</param>
        /// <param name="y">Value on y-coordinate.</param>
        public Point(RGFloat x, RGFloat y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Compare two points to check whether or not they are same.
        /// </summary>
        /// <param name="obj">Another object to be compared with this point.</param>
        /// <returns>True if two points are same; Otherwise return false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Point)) return false;

            var size2 = (Point)obj;

            return this.X == size2.X && this.Y == size2.Y;
        }

        /// <summary>
        /// Get hash code of this point.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Convert point into string. (Format: {x, y})
        /// </summary>
        /// <returns>String converted from this point.</returns>
        public override string ToString()
        {
            return string.Format("{{{0,2}, {1,2}}}", this.X, this.Y);
        }

        /// <summary>
        /// Compare two points to check whether or not they are same.
        /// </summary>
        /// <param name="size1">First point to be compared.</param>
        /// <param name="size2">Second point to be compared.</param>
        /// <returns>True if two points are same; Otherwise return false.</returns>
        public static bool operator ==(Point size1, Point size2)
        {
            return size1.X == size2.X && size1.Y == size2.Y;
        }

        /// <summary>
        /// Compare two points to check whether or not they are not same.
        /// </summary>
        /// <param name="size1">First point to be compared.</param>
        /// <param name="size2">Second point to be compared.</param>
        /// <returns>True if two points are not same; Otherwise return false.</returns>
        public static bool operator !=(Point size1, Point size2)
        {
            return size1.X != size2.X || size1.Y != size2.Y;
        }

        /// <summary>
        /// Transform point by specified matrix.
        /// </summary>
        /// <param name="p">Point to be transformed.</param>
        /// <param name="m">Matrix used to calculate the result of transform.</param>
        /// <returns>A transformed point from specified matrix.</returns>
        public static Point operator *(Point p, Matrix3x2f m)
        {
            return new Point(p.X * m.a1 + p.Y * m.a2 + m.a3, p.X * m.a1 + p.Y * m.b2 + m.b3);
        }

        public static implicit operator Avalonia.Point(Point p)
        {
            return new Avalonia.Point(p.X, p.Y);
        }
        public static implicit operator Point(Avalonia.Point p)
        {
            return new Point(p.X, p.Y);
        }
    }

}


