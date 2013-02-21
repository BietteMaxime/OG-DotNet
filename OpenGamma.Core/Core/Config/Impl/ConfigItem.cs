// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigItem.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge.Serialization;
using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Core.Config.Impl
{
    [FudgeSurrogate(typeof(ConfigItemBuilder))]
    public class ConfigItem<TConfig>
    {
        private readonly UniqueId _uniqueId;
        private readonly string _name;
        private readonly TConfig _value;

        public ConfigItem(TConfig value, string name)
            : this(null, value, name)
        {
        }

        public ConfigItem(UniqueId uniqueId, TConfig value, string name)
        {
            _uniqueId = uniqueId;
            _value = value;
            _name = name;
        }

        public TConfig Value
        {
            get { return _value; }
        }

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
        }

        public string Name
        {
            get { return _name; }
        }
    }

    public class ConfigItem
    {
        private ConfigItem()
        {
        }

        public static ConfigItem<T> Create<T>(T value, string name)
        {
            return new ConfigItem<T>(null, value, name);
        }
    }
}
