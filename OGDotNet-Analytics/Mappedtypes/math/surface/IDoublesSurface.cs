//-----------------------------------------------------------------------
// <copyright file="IDoublesSurface.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.math.surface
{
    public interface IDoublesSurface
    {
        string Name { get; }
        double[] XData { get; }
        double[] YData { get; }
        double[] ZData { get; }

        double GetZValue(double x, double y);
    }
}
