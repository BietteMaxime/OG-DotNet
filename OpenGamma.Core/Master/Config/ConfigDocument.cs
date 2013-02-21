// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge.Serialization;

using OpenGamma.Core.Config.Impl;
using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Master.Config
{
    [FudgeSurrogate(typeof(ConfigDocumentBuilder))]
    public class ConfigDocument<TConfig> : AbstractDocument
    {
        private UniqueId _uniqueId;
        private ConfigItem<TConfig> _config;
        private string _name;

        public ConfigDocument(ConfigItem<TConfig> config)
        {
            setConfig(config);
        }

        public ConfigDocument(UniqueId uniqueId, ConfigItem<TConfig> config, string name, DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant)
            : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
            _uniqueId = uniqueId;
            _name = name;
            setConfig(config);
        }

        public override UniqueId UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public ConfigItem<TConfig> Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public void setConfig(ConfigItem<TConfig> config)
        {
            _config = config;
            if (config != null)
            {
                if (_name == null)
                {
                    _name = config.Name;
                }

                if (_uniqueId == null)
                {
                    _uniqueId = config.UniqueId;
                }
            }
        }
    }
}
