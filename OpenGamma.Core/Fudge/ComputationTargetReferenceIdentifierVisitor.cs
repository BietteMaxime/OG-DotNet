// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationTargetReferenceIdentifierVisitor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine;
using OpenGamma.Engine.Target;

namespace OpenGamma.Fudge
{
    internal class ComputationTargetReferenceIdentifierVisitor : IComputationTargetReferenceVisitor<object>
    {
        public object VisitComputationTargetSpecification(ComputationTargetSpecification specification)
        {
            return specification.UniqueId;
        }

        public object VisitComputationTargetRequirement(ComputationTargetRequirement requirement)
        {
            return requirement.Identifiers;
        }
    }
}