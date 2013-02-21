// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteEngineContextTestBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using OpenGamma.Engine.View;
using OpenGamma.Id;
using OpenGamma.Model.Context;

namespace OpenGamma.Model.Resources
{
    public abstract class RemoteEngineContextTestBase
    {
        private static readonly Lazy<RemoteEngineContext> ContextLazy = new Lazy<RemoteEngineContext>(GetContext);

        protected static RemoteEngineContext Context
        { 
            get { return ContextLazy.Value; }
        }

        private static RemoteEngineContext GetContext()
        {
            return RemoteEngineContextTests.GetContext();
        }

        public UniqueId GetViewDefinitionId(string name)
        {
            return GetViewDefinition(name).UniqueId;
        }

        public ViewDefinition GetViewDefinition(string name)
        {
            var viewDefinition = Context.ConfigSource.Get<ViewDefinition>(name);
            if (viewDefinition == null)
            {
                throw new OpenGammaException(string.Format("No view definition with name '{0}' could be found", name));
            }
            return viewDefinition;
        }
    }
}
