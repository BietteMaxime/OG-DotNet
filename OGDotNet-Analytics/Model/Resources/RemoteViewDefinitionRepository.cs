//-----------------------------------------------------------------------
// <copyright file="RemoteViewDefinitionRepository.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Model.Resources
{
    public class RemoteViewDefinitionRepository
    {
        private readonly RestTarget _rest;

        public RemoteViewDefinitionRepository(RestTarget rest)
        {
            _rest = rest;
        }

        public IEnumerable<string> GetDefinitionNames()
        {
            return GetDefinitionEntries().Values.OrderBy(s => s).ToList();
        }

        public IEnumerable<ObjectId> GetDefinitionIDs()
        {
            var fudgeMsg = _rest.Resolve("ids").GetFudge();
            return fudgeMsg.GetAllByOrdinal(1).Select(f => (string)f.Value).Select(ObjectId.Parse).ToList();
        }

        public Dictionary<UniqueId, string> GetDefinitionEntries()
        {
            var fudgeMsg = _rest.GetFudge();
            return MapBuilder.FromFudgeMsg(fudgeMsg, f => UniqueId.Parse((string)f.Value), f => (string)f.Value);
        }
        public ViewDefinition GetViewDefinition(string definitionName)
        {
            return _rest.Resolve("name").Resolve(definitionName).Get<ViewDefinition>();
        }
        public ViewDefinition GetViewDefinition(UniqueId id)
        {
            return _rest.Resolve("id").Resolve(id.ToString()).Get<ViewDefinition>();
        }
    }
}