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

using System.ComponentModel.Composition;
using Atdl4net.Model.Controls;
using Common.Logging;

namespace Atdl4net.Wpf.View.DefaultRendering
{
    [Export(typeof(IWpfControlRenderer<RadioButton_t>))]
    internal class RadioButtonRenderer : IWpfControlRenderer<RadioButton_t>
    {
        private static readonly ILog _log = LogManager.GetLogger("Atdl4net.Wpf.View");

        public void Render(WpfXmlWriter writer, RadioButton_t control)
        {
            string id = WpfControlRenderer.CleanName(control.Id);

            _log.Debug(m => m("Rendering control {0} of type RadioButton_t using {1}", control.Id, this.GetType().FullName));

            using (writer.New(WpfXmlWriterTag.RadioButton))
            {
                WpfControlRenderer.WriteGridAttribute(writer, control);

                writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "1,5,4,1");

                if (!string.IsNullOrEmpty(control.Label))
                    writer.WriteAttribute(WpfXmlWriterAttribute.Content, control.Label);

                if (!string.IsNullOrEmpty(control.Id))
                    writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);

                // For .NET 4.0 we can rely on GroupName, but for .NET 3.5 we have to provide our own mechanism to 
                // ensure that only one radio button is enabled at a time
#if NET_40
                if (!string.IsNullOrEmpty(control.RadioGroup))
                    writer.WriteAttribute(WpfXmlWriterAttribute.GroupName, WpfControlRenderer.CleanName(control.RadioGroup));
#endif

                writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, string.Format("{0}Binding Path=Controls[{1}].ToolTip{2}", "{", id, "}"));
                writer.WriteAttribute(WpfXmlWriterAttribute.IsChecked, string.Format("{0}Binding Path=Controls[{1}].UiValue{2}", "{", id, "}"));
                writer.WriteAttribute(WpfXmlWriterAttribute.IsEnabled, string.Format("{0}Binding Path=Controls[{1}].Enabled{2}", "{", id, "}"));
                writer.WriteAttribute(WpfXmlWriterAttribute.Visibility, string.Format("{0}Binding Path=Controls[{1}].Visibility{2}", "{", id, "}"));
            }
        }
    }
}
