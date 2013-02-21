// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Fudge;
using Fudge.Serialization;
using Fudge.Types;

using OpenGamma.Util;

namespace OpenGamma.Fudge
{
    public static class MapBuilder
    {
        public static Dictionary<TKey, TValue> FromFudgeMsg<TKey, TValue>(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
            where TKey : class
            where TValue : class
        {
            return FromFudgeMsg(ffc, deserializer.FromField<TKey>, deserializer.FromField<TValue>);
        }

        public static Dictionary<TKey, TValue> FromFudgeMsg<TKey, TValue>(IFudgeFieldContainer ffc, Func<IFudgeField, TKey> keyFactory, Func<IFudgeField, TValue> valueFactory)
        {
            ArgumentChecker.NotNull(keyFactory, "keyFactory");
            ArgumentChecker.NotNull(valueFactory, "valueFactory");
            if (ffc == null)
            {
                return new Dictionary<TKey, TValue>();
            }

            if (ffc.Any(f => f.Ordinal.GetValueOrDefault(0) > 2))
            {
                throw new ArgumentException();
            }

            var entries = ffc.GetAllByOrdinal(1).Zip(ffc.GetAllByOrdinal(2), Tuple.Create);
            return entries.ToDictionary(t => keyFactory(t.Item1), t => valueFactory(t.Item2));
        }

        public static global::Fudge.FudgeMsg ToFudgeMsg<TKey, TValue>(IFudgeSerializer s, IDictionary<TKey, TValue> dict, Func<TKey, object> keyMsgGen = null, Func<TValue, object> valueMsgGen = null) where TValue : class
        {
            var fudgeSerializer = new FudgeSerializer(s.Context);

            keyMsgGen = keyMsgGen ?? (k => fudgeSerializer.SerializeToMsg(k));
            valueMsgGen = valueMsgGen ?? (v => fudgeSerializer.SerializeToMsg(v));

            var valuesMessage = new global::Fudge.FudgeMsg(s.Context);
            foreach (var value in dict)
            {
                valuesMessage.Add(1, keyMsgGen(value.Key));
                if (value.Value == null)
                {
                    valuesMessage.Add(2, IndicatorType.Instance);
                }
                else
                {
                    valuesMessage.Add(2, valueMsgGen(value.Value));
                }
            }

            return valuesMessage;
        }
    }
}
