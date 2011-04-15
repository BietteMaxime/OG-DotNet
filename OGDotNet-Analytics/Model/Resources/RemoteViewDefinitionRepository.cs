﻿//-----------------------------------------------------------------------
// <copyright file="RemoteViewDefinitionRepository.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.engine.view;

namespace OGDotNet.Model.Resources
{
    public class RemoteViewDefinitionRepository
    {
        private readonly OpenGammaFudgeContext _context;
        private readonly RestTarget _rest;
        private readonly string _activeMqSpec;

        public RemoteViewDefinitionRepository(OpenGammaFudgeContext context, RestTarget rest, string activeMqSpec)
        {
            _context = context;
            _rest = rest;
            _activeMqSpec = activeMqSpec;
        }

        public IEnumerable<string> GetDefinitionNames()
        {
            var fudgeMsg = _rest.GetFudge();

            return fudgeMsg.GetAllByOrdinal(1).Select(fudgeField => (string)fudgeField.Value).ToList();
        }
        public ViewDefinition GetViewDefinition(string definitionName)
        {
            return _rest.Resolve(definitionName).Get<ViewDefinition>();
        }
    }
}