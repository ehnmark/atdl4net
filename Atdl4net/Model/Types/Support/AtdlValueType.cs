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

using System;
using System.Linq;
using Atdl4net.Diagnostics.Exceptions;
using Atdl4net.Model.Controls.Support;
using Atdl4net.Model.Elements.Support;
using Atdl4net.Resources;
using ThrowHelper = Atdl4net.Diagnostics.ThrowHelper;

namespace Atdl4net.Model.Types.Support
{
    /// <summary>
    /// Base class for all value type parameters (Int_t, Float_t, etc.).
    /// </summary>
    /// <remarks>Parameter types must be one of <see cref="AtdlValueType{T}"/> or <see cref="AtdlReferenceType{T}"/>.
    /// The reason for the differentiation is that most FIXatdl types that use value types for the underlying storage
    /// (Int_t, Float_t, UTCTimestamp_t, etc.) actually use <see cref="Nullable{T}"/> so that they can also contain
    /// null, meaning don't include this value in the FIX output.  However, Nullable&lt;T&gt; is a value type, not
    /// a reference type, and so a different base type is required to support underlying reference type usage, such
    /// as in String_t.  (This is the same reason that it isn't possible to factor out apparently duplicated code
    /// across AtdlValueType&lt;T&gt; and AtdlReferenceType&lt;T&gt;, because one uses T? internally and the
    /// other uses T.)</remarks>
    public abstract class AtdlValueType<T> : IParameterType where T : struct
    {
        /// <summary>
        /// Storage for the value of this parameter, as type T?.
        /// </summary>
        protected T? _value;

        /// <summary>
        /// Gets/sets an optional constant value for this parameter.
        /// </summary>
        /// <value>The const value.</value>
        public T? ConstValue { get; set; }

        #region IParameterType Members

        /// <summary>
        /// Gets the value of this parameter as seen by the Control_t that references it.  May be null if the 
        /// parameter has no value, for example if it has explicitly been set via a state rule to {NULL}.
        /// </summary>
        /// <param name="hostParameter"><see cref="IParameter"/> that hosts the value.</param>
        /// <remarks>An <see cref="IControlConvertible"/> is returned enabling the parameter value to be converted into any 
        /// desired type, provided that the underlying value supports that type.</remarks>
        public IControlConvertible GetValueForControl(IParameter hostParameter)
        {
            // This base type doesn't know how to convert to control value types, but derived types must
            // implement IControlConvertible.
            return this as IControlConvertible;
        }

        /// <summary>
        /// Sets the value of this parameter as seen by the Control_t that references it.
        /// </summary>
        /// <param name="hostParameter"><see cref="IParameter"/> that hosts the value.</param>
        /// <param name="value">Control value that implements <see cref="IParameterConvertible"/>.</param>
        /// <remarks>An <see cref="IParameterConvertible"/> is passed in enabling the control value to be converted into any 
        /// desired type, provided that the value supports conversion to that type.</remarks>
        public void SetValueFromControl(IParameter hostParameter, IParameterConvertible value)
        {
            if (ConstValue != null)
                throw ThrowHelper.New<InvalidOperationException>(this, ErrorMessages.AttemptToSetConstValueParameter, ConstValue);
            try
            {
                _value = ValidateValue(ConvertToNativeType(hostParameter, value));
            }
            catch (ArgumentException ex)
            {
                throw ThrowHelper.New<InvalidFieldValueException>(this, ex, ex.Message);
            }
            catch (InvalidCastException ex)
            {
                throw ThrowHelper.New<InvalidFieldValueException>(this, ex, ex.Message);
            }
        }

        /// <summary>
        /// Sets the wire value for this parameter.  This method is typically used to initialise the parameter through the
        /// InitValue mechanism, but may also be used to initialise the parameter when doing order amendments.
        /// </summary>
        /// <param name="hostParameter"><see cref="Atdl4net.Model.Elements.Parameter_t{T}"/> that is hosting this type. 
        /// Parameters in Atdl4net are represented by means of the generic Parameter_t type with the appropriate type parameter, 
        /// for example, Parameter_t&lt;Amt_t&gt;.</param>
        /// <param name="value">New wire value (all wire values in Atdl4net are strings).</param>
        public void SetWireValue(IParameter hostParameter, string value)
        {
            // When ConstValue is set, the only assignment we allow is if the supplied value is the same value as ConstValue.
            if (ConstValue != null)
            {
                if (ConvertToWireValueFormat(ConstValue) == value)
                    return;

                throw ThrowHelper.New<InvalidOperationException>(this, ErrorMessages.AttemptToSetConstValueParameter, ConstValue);
            }

            _value = ValidateValue(ConvertFromWireValueFormat(value));
        }

        /// <summary>
        /// Gets the wire value for this parameter.  This method is used to retrieve the value of the parameter that should
        /// be transmitted over FIX.
        /// </summary>
        /// <param name="hostParameter"><see cref="Parameter_t{T}"/> that is hosting this type.  Parameters in Atdl4net are
        /// represented by means of the generic Parameter_t type with the appropriate type parameter, for example, 
        /// Parameter_t&lt;Amt_t&gt;.</param>
        /// <returns>The parameter's current wire value (all wire values in Atdl4net are strings).</returns>
        public string GetWireValue(IParameter hostParameter)
        {
            return (ConstValue != null) ? ConvertToWireValueFormat(ConstValue) : ConvertToWireValueFormat(_value);
        }

        /// <summary>
        /// Gets the value of this parameter type in its native (i.e., raw) form, such as int, char, string, etc. 
        /// </summary>
        /// <returns>Native parameter value.</returns>
        public object GetNativeValue()
        {
            return _value;
        }
        #endregion

        #region Abstract Methods that all FIXatdl value-based types must implement

        /// <summary>
        /// Validates the supplied value in terms of the parameters constraints (e.g., MinValue, MaxValue, etc.).
        /// </summary>
        /// <param name="value">Value to validate, may be null in which case no validation is applied.</param>
        /// <returns>Value passed in is returned if it is valid; otherwise an appropriate exception is thrown.</returns>
        protected abstract T? ValidateValue(T? value);

        /// <summary>
        /// Converts the supplied value from string format (as might be used on the FIX wire) into the type of the type
        /// parameter for this type.  
        /// </summary>
        /// <param name="value">Type to convert from string, may be null.</param>
        /// <returns>If input value is not null, returns value converted from a string; null otherwise.</returns>
        protected abstract T? ConvertFromWireValueFormat(string value);

        /// <summary>
        /// Converts the supplied value to a string, as might be used on the FIX wire.
        /// </summary>
        /// <param name="value">Value to convert, may be null.</param>
        /// <returns>If input value is not null, returns value converted to a string; null otherwise.</returns>
        protected abstract string ConvertToWireValueFormat(T? value);

        /// <summary>
        /// Converts the supplied value to the type parameter type (T?) for this class.
        /// </summary>
        /// <param name="hostParameter"><see cref="IParameter"/> that hosts this value.</param>
        /// <param name="value">Value to convert, may be null.</param>
        /// <returns>If input value is not null, returns value converted to T?; null otherwise.</returns>
        protected abstract T? ConvertToNativeType(IParameter hostParameter, IParameterConvertible value);

        #endregion
    }
}