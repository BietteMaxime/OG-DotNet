//-----------------------------------------------------------------------
// <copyright file="FudgeListWrapper.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;

namespace OGDotNet.Mappedtypes.Util.FudgeMsg
{
    /// <summary>
    /// Fudge type mapping doesn't work if I make this generic
    /// </summary>
    public class FudgeListWrapper<T>
    {
        private readonly List<T> _list;

        public FudgeListWrapper(List<T> list)
        {
            _list = list;
        }

        public List<T> List
        {
            get { return _list; }
        }
    }
}