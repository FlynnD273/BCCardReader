/*
 *  Copyright (c) 2013-2016, Cureos AB.
 *  All rights reserved.
 *  http://www.cureos.com
 *
 *	This file is part of Shim.NET.
 *
 *  Shim.NET is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Shim.NET is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Shim.NET.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace System.Drawing
{
    using System.Linq;

    using CoreGraphics;

    public partial struct Color
    {
        #region OPERATORS

        public static implicit operator CGColor(Color color)
        {
            return new CGColor(color._r / 255.0f, color._g / 255.0f, color._b / 255.0f, color._a / 255.0f);
        }

        public static implicit operator Color(CGColor cgColor)
        {
            var components = cgColor.Components.Select(component => (int)(255.0f * component)).ToList();
            switch (cgColor.NumberOfComponents)
            {
                case 2:
                    return FromArgb(components[1], components[0], components[0], components[0]);
                case 4:
                    return FromArgb(components[3], components[0], components[1], components[2]);
                default:
                    throw new ArgumentException("Invalid number of color components", "cgColor");
            }
        }

        #endregion
    }
}