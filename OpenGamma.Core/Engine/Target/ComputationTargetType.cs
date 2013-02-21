// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationTargetType.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge;
using Fudge.Serialization;
using OpenGamma.Core.Position;
using OpenGamma.Core.Security;
using OpenGamma.Id;
using OpenGamma.Util.Money;

namespace OpenGamma.Engine.Target
{
    public abstract class ComputationTargetType
    {
        public static readonly ClassComputationTargetType<IPortfolio> Portfolio = DefaultClass<IPortfolio>("PORTFOLIO");
        public static readonly ClassComputationTargetType<IPortfolioNode> PortfolioNode = DefaultClass<IPortfolioNode>("PORTFOLIO_NODE");
        public static readonly ClassComputationTargetType<IPosition> Position = DefaultClass<IPosition>("POSITION");
        public static readonly ClassComputationTargetType<ISecurity> Security = DefaultClass<ISecurity>("SECURITY");
        public static readonly ClassComputationTargetType<ITrade> Trade = DefaultClass<ITrade>("TRADE");
        public static readonly ClassComputationTargetType<UniqueId> Primitive = DefaultClass<UniqueId>("PRIMITIVE");
        public static readonly ClassComputationTargetType<Currency> Currency = DefaultClass<Currency>("CURRENCY");
        public static readonly ClassComputationTargetType<UnorderedCurrencyPair> UnorderedCurrencyPair = DefaultClass<UnorderedCurrencyPair>("UNORDERED_CURRENCY_PAIR");
        public static readonly ComputationTargetType Null = new NullComputationTargetType();
        public static readonly ComputationTargetType LegacyPrimitive = Primitive.Or(Currency).Or(UnorderedCurrencyPair);

        private static ClassComputationTargetType<TTarget> Create<TTarget>(string name, bool nameWellKnown) where TTarget : IUniqueIdentifiable
        {
            return new ClassComputationTargetType<TTarget>(name, nameWellKnown);
        }

        private static ClassComputationTargetType<TTarget> DefaultClass<TTarget>(string name) where TTarget : class, IUniqueIdentifiable
        {
            return Create<TTarget>(name, true);
        }

        public ComputationTargetType Containing(ComputationTargetType inner)
        {
            return new NestedComputationTargetType(this, inner);
        }

        public ComputationTargetType Or(ComputationTargetType alternative)
        {
            return new MultipleComputationTargetType(this, alternative);
        }
        
        public abstract void Serialize(string fieldName, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer);
    }
}