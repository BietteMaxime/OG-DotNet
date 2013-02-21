// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IComputationTargetReferenceVisitor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenGamma.Engine.Target
{
    public interface IComputationTargetReferenceVisitor<out TResult>
    {
        TResult VisitComputationTargetSpecification(ComputationTargetSpecification specification);
        TResult VisitComputationTargetRequirement(ComputationTargetRequirement requirement);
    }
}