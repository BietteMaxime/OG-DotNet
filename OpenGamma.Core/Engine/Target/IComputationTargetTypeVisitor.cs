// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IComputationTargetTypeVisitor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace OpenGamma.Engine.Target
{
    public interface IComputationTargetTypeVisitor<in TData>
    {
        void VisitMultipleComputationTargetTypes(ISet<ComputationTargetType> types, TData data);
        void VisitNestedComputationTargetTypes(IList<ComputationTargetType> types, TData data);
        void VisitNullComputationTargetType(TData data);
        void VisitClassComputationTargetType(Type type, string name, bool nameWellKnown, TData data);
    }
}
