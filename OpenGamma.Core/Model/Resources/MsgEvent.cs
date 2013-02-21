// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsgEvent.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace OpenGamma.Model.Resources
{
    public class MsgEvent : EventArgs
    {
        private readonly object _msg;

        public MsgEvent(object msg)
        {
            _msg = msg;
        }

        public object Msg
        {
            get { return _msg; }
        }
    }
}