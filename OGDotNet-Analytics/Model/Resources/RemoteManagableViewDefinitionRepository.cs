//-----------------------------------------------------------------------
// <copyright file="RemoteManagableViewDefinitionRepository.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Financial.View;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Model.Resources
{
    public class RemoteManagableViewDefinitionRepository : RemoteViewDefinitionRepository
    {
        private readonly RestTarget _rest;

        public RemoteManagableViewDefinitionRepository(RestTarget rest) : base(rest)
        {
            _rest = rest;
        }

        public void AddViewDefinition(AddViewDefinitionRequest request)
        {
            _rest.Post(request);
        }

        public void UpdateViewDefinition(UpdateViewDefinitionRequest request)
        {
            _rest.Resolve(request.Name).Put(request);
        }

        //TODO [Obsolete("Use the view UniqueId")]
        public void RemoveViewDefinition(string name)
        {
            ViewDefinition viewDefinition;
            try
            {
                viewDefinition = GetViewDefinition(name);
            }
            catch (NullReferenceException)
            {
                throw new DataNotFoundException();
            }
            if (viewDefinition == null)
            {
                throw new DataNotFoundException();
            }
            var uid = viewDefinition.UniqueID;
            RemoveViewDefinition(uid);
        }

        public void RemoveViewDefinition(UniqueId id)
        {
            _rest.Resolve("id").Resolve(id.ToString()).Delete();
        }
    }
}