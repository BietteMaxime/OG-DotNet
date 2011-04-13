//-----------------------------------------------------------------------
// <copyright file="ResultOutputMode.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph
{
    public enum ResultOutputMode
    {
        //TODO shouldOutputResult
        None,
        TerminalOutputs,
        All
    }
}
