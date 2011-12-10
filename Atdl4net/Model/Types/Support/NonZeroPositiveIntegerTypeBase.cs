﻿#region Copyright (c) 2010-2011, Cornerstone Technology Limited. http://atdl4net.org
//
//   This software is released under both commercial and open-source licenses.
//
//   If you received this software under the commercial license, the terms of that license can be found in the
//   Commercial.txt file in the Licenses folder.  If you received this software under the open-source license,
//   the following applies:
//
//      This file is part of Atdl4net.
//
//      Atdl4net is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public 
//      License as published by the Free Software Foundation, either version 2.1 of the License, or (at your option) any later version.
// 
//      Atdl4net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
//      of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more details.
//
//      You should have received a copy of the GNU Lesser General Public License along with Atdl4net.  If not, see
//      http://www.gnu.org/licenses/.
//
#endregion

using Atdl4net.Resources;
using System;
using ThrowHelper = Atdl4net.Diagnostics.ThrowHelper;

namespace Atdl4net.Model.Types.Support
{
    /// <summary>
    /// Abstract base class for FIXatdl types that require positive integers greater than zero.
    /// </summary>
    public abstract class NonZeroPositiveIntegerTypeBase : NonNegativeIntegerTypeBase
    {
        /// <summary>
        /// Validates the supplied value in terms of the parameters constraints (e.g., MinValue, MaxValue, etc.).
        /// </summary>
        /// <param name="value">Value to validate, may be null in which case no validation is applied.</param>
        /// <returns>Value passed in is returned if it is valid; otherwise an appropriate exception is thrown.</returns>
        protected override uint? ValidateValue(uint? value)
        {
            if (value != null && (uint)value < 1)
                throw ThrowHelper.New<ArgumentOutOfRangeException>(this, ErrorMessages.NonZeroPositiveIntRequired, value);

            return value;
        }
    }
}