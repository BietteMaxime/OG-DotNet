// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageablePortfolio.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;
using OpenGamma.Util;

namespace OpenGamma.Master.Portfolio
{
    [FudgeSurrogate(typeof(ManageablePortfolioBuilder))]
    public class ManageablePortfolio : IUniqueIdentifiable
    {
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageablePortfolio" /> class.
        /// </summary>
        public ManageablePortfolio()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageablePortfolio" /> class, specifying the name.
        /// </summary>
        /// <param name="name">the name, not null</param>
        public ManageablePortfolio(string name)
        {
            ArgumentChecker.NotNull(name, "name");
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageablePortfolio" /> class, specifying the name and root node.
        /// </summary>
        /// <param name="name">the name, not null</param>
        /// <param name="rootNode">the root node, not null</param>
        public ManageablePortfolio(string name, ManageablePortfolioNode rootNode)
        {
            ArgumentChecker.NotNull(name, "name");
            ArgumentChecker.NotNull(rootNode, "rootNode");
            Name = name;
            RootNode = rootNode;
        }

        /// <summary>
        /// Adds a key/value pair to the attributes.
        /// </summary>
        /// <param name="key">the key, not null</param>
        /// <param name="value">the value, not null</param>
        public void AddAttribute(string key, string value)
        {
            ArgumentChecker.NotNull(key, "key");
            ArgumentChecker.NotNull(value, "value");
            _attributes.Add(key, value);
        }

        public UniqueId UniqueId { get; set; }
        public string Name { get; set; }
        public ManageablePortfolioNode RootNode { get; set; }
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