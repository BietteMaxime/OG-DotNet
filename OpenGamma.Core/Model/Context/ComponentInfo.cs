// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComponentInfo.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace OpenGamma.Model.Context
{
    public class ComponentInfo
    {
        private readonly ComponentKey _key;
        private readonly Uri _uri;
        private readonly Dictionary<string, string> _attributes;

        public ComponentInfo(ComponentKey key, Uri uri, Dictionary<string, string> attributes)
        {
            _key = key;
            _attributes = attributes;
            _uri = uri;
        }

        public ComponentKey Key
        {
            get { return _key; }
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public Dictionary<string, string> Attributes
        {
            get { return _attributes; }
        }

        public override string ToString()
        {
            return string.Format("[ComponentInfo {0}/{1}]", _uri, _key);
        }
    }
}
