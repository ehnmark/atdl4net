﻿#region Copyright (c) 2010-2012, Cornerstone Technology Limited. http://atdl4net.org
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
using System.Collections.ObjectModel;
using System.Linq;
using Atdl4net.Model.Collections;
using Atdl4net.Model.Elements;

namespace Atdl4net.Wpf.ViewModel
{
    /// <summary>
    /// Collection of <see cref="StrategyEditWrapper"/>s, part of the Atdl4net ViewModel.
    /// </summary>
    public class ViewModelStrategyEditCollection : Collection<StrategyEditWrapper>, IDisposable
    {
        private bool _disposed;
        private ViewModelControlCollection _controls;

        /// <summary>
        /// Initializes a new <see cref="ViewModelStrategyEditCollection"/>.
        /// </summary>
        /// <param name="underlyingStrategyEdits">Set of <see cref="StrategyEdit_t"/>s that this collection is responsible for.</param>
        /// <param name="controls">Collection of controls for the strategy this <see cref="ViewModelStrategyEditCollection"/>
        /// corresponds to.</param>
        public ViewModelStrategyEditCollection(StrategyEditCollection underlyingStrategyEdits, ViewModelControlCollection controls)
        {
            foreach (StrategyEdit_t strategyEdit in underlyingStrategyEdits) 
            {
                _controls = controls;

                StrategyEditWrapper strategyEditWrapper = new StrategyEditWrapper(strategyEdit);

                Add(strategyEditWrapper);

                strategyEditWrapper.Bind(from c in _controls 
                                         where c.ParameterRef != null && strategyEdit.Sources.Contains(c.ParameterRef) 
                                         select c);
            }
        }

        #region IDisposable Members and support

        void IDisposable.Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleans up when this <see cref="ViewModelStrategyEditCollection"/> is no longer required.
        /// </summary>
        /// <param name="disposing">True if this object is being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (StrategyEditWrapper strategyEdit in this)
                    {
                        foreach (ControlWrapper control in _controls)
                        {
                            string targetParameter = control.UnderlyingControl.ParameterRef;

                            if (strategyEdit.Sources.Contains(targetParameter))
                                control.UnbindStrategyEdit(strategyEdit);
                        }
                    }

                    _controls = null;
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
