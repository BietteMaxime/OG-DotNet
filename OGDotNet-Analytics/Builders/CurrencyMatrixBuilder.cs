//-----------------------------------------------------------------------
// <copyright file="CurrencyMatrixBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.financial.currency;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    internal class CurrencyMatrixBuilder : BuilderBase<CurrencyMatrix>
    {
        private const string UniqueIDFieldName = "uniqueId";
        private const string FixedRateFieldName = "fixedRate";
        private const string ValueRequirementsFieldName = "valueReq";
        private const string CrossConvertFieldName = "crossConvert";

        public CurrencyMatrixBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override CurrencyMatrix DeserializeImpl(IFudgeFieldContainer message, IFudgeDeserializer deserializer)
        {
            var matrixImpl = new MatrixImpl();

            IFudgeField field = message.GetByName(UniqueIDFieldName);
            if (field != null)
            {
                var uid = UniqueIdentifier.Parse((string)field.Value);
                matrixImpl.UniqueId = uid;
            }
            field = message.GetByName(CrossConvertFieldName);
            if (field != null)
            {
                var crossconvert = (FudgeMsg)field.Value;
                matrixImpl.LoadCross(crossconvert);
            }
            field = message.GetByName(FixedRateFieldName);
            if (field != null)
            {
                throw new NotImplementedException();
            }
            field = message.GetByName(ValueRequirementsFieldName);
            if (field != null)
            {
                FudgeMsg value = (FudgeMsg)field.Value;
                matrixImpl.LoadReq(value, deserializer);
            }

            return matrixImpl;
        }

        private class MatrixImpl : CurrencyMatrix
        {
            private readonly ConcurrentDictionary<Currency, Dictionary<Currency, CurrencyMatrixValue>> _values = new ConcurrentDictionary<Currency, Dictionary<Currency, CurrencyMatrixValue>>();

            public UniqueIdentifier UniqueId { get; internal set; }

            public override ICollection<Currency> SourceCurrencies
            {
                get { return _values.Keys; }
            }

            public override IEnumerable<Currency> TargetCurrencies
            {
                get { return _values.SelectMany(e => e.Value.Keys).Distinct(); } //TODO this less slowly
            }

            public override CurrencyMatrixValue GetConversion(Currency source, Currency target)
            {
                if (source.Equals(target))
                {
                    // This shouldn't happen in sensible code
                    return CurrencyMatrixValue.Of(1.0);
                }
                Dictionary<Currency, CurrencyMatrixValue> targets;
                if (_values.TryGetValue(source, out targets))
                {
                    CurrencyMatrixValue ret;
                    if (targets.TryGetValue(target, out ret))
                    {
                        return ret;
                    }
                }
                return null;
            }

            public void LoadCross(FudgeMsg message)
            {
                var values = new Dictionary<Tuple<Currency, Currency>, CurrencyMatrixValue>();

                foreach (IFudgeField field in message)
                {
                    CurrencyMatrixValue cross = CurrencyMatrixValue.Of(Currency.Create(field.Name));
                    foreach (IFudgeField field2 in (IFudgeFieldContainer)field.Value)
                    {
                        Currency source = Currency.Create(field2.Name);
                        if (field2.Value is IFudgeFieldContainer)
                        {
                            Currency target = Currency.Create(((IFudgeFieldContainer)field2.Value).First().Name);
                            values.Add(Tuple.Create(source, target), cross);
                        }
                        else
                        {
                            Currency target = Currency.Create((string)field2.Value);
                            values.Add(Tuple.Create(source, target), cross);
                            values.Add(Tuple.Create(target, source), cross);
                        }
                    }
                    foreach (var valueEntry in values)
                    {
                        AddConversion(valueEntry.Key.Item1, valueEntry.Key.Item1, valueEntry.Value);
                    }
                }
            }

            public void LoadReq(FudgeMsg message, IFudgeDeserializer deserializer)
            {
                var values = new Dictionary<Tuple<Currency, Currency>, CurrencyMatrixValue>();
                foreach (var field in message)
                {
                    Currency source = Currency.Create(field.Name);
                    foreach (var field2 in (IFudgeFieldContainer)field.Value)
                    {
                        Currency target = Currency.Create(field2.Name);

                        if (field2.Value is IFudgeFieldContainer)
                        {
                            CurrencyMatrixValue value = deserializer.FromField<CurrencyMatrixValue.CurrencyMatrixValueRequirement>(field2);
                            values.Add(Tuple.Create(source, target), value);
                            values.Add(Tuple.Create(target, source), value.GetReciprocal());
                        }
                        else
                        {
                            values.Remove(Tuple.Create(target, source));
                        }
                    }
                }
                foreach (var valueEntry in values)
                {
                    AddConversion(valueEntry.Key.Item1, valueEntry.Key.Item2, valueEntry.Value);
                }
            }

            private void AddConversion(Currency source, Currency target, CurrencyMatrixValue rate)
            {
                Dictionary<Currency, CurrencyMatrixValue> conversions = _values.GetOrAdd(source, new Dictionary<Currency, CurrencyMatrixValue>());
                conversions[target] = rate;
            }
        }
    }
}