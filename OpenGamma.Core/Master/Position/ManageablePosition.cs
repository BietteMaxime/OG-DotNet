// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageablePosition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;
using OpenGamma.Master.Security;
using OpenGamma.Util;

namespace OpenGamma.Master.Position
{
    [FudgeSurrogate(typeof(ManageablePositionBuilder))]
    public class ManageablePosition : IUniqueIdentifiable
    {
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();

        public UniqueId UniqueId { get; set; }
        public decimal Quantity { get; set; }
        public ManageableSecurityLink SecurityLink { get; set; }

        public void AddAttribute(string key, string value)
        {
            ArgumentChecker.NotNull(key, "key");
            ArgumentChecker.NotNull(value, "value");
            _attributes.Add(key, value);
        }

        public IDictionary<string, string> Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes.Clear();
                foreach (KeyValuePair<string, string> kv in value)
                {
                    _attributes.Add(kv.Key, kv.Value);
                }
            }
        }
    }
}
