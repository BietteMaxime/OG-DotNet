//-----------------------------------------------------------------------
// <copyright file="MapBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Builders
{
    public static class MapBuilder
    {
        public static Dictionary<TKey, TValue> FromFudgeMsg<TKey, TValue>(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer, string msgName, Func<IFudgeField, TKey> keyFactory = null, Func<IFudgeField, TValue> valueFactory = null)
            where TKey : class
            where TValue : class
        {
            IFudgeFieldContainer dictMessage = ffc.GetMessage(msgName);
            return FromFudgeMsg(dictMessage, deserializer, keyFactory, valueFactory);
        }

        public static Dictionary<TKey, TValue> FromFudgeMsg<TKey, TValue>(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer,  Func<IFudgeField, TKey> keyFactory = null, Func<IFudgeField, TValue> valueFactory = null) where TKey : class where TValue : class
        {
            if (ffc == null)
            {
                return new Dictionary<TKey, TValue>();
            }
            keyFactory = keyFactory ?? deserializer.FromField<TKey>;
            valueFactory = valueFactory ?? deserializer.FromField<TValue>;

            if (ffc.Any(f=>f.Ordinal.GetValueOrDefault(0) > 2))
            {
                throw new ArgumentException();
            }

            return ffc.GetAllByOrdinal(1).Zip(ffc.GetAllByOrdinal(2), Tuple.Create)
                .ToDictionary(t => keyFactory(t.Item1), t => valueFactory(t.Item2))
                ;
        }

        public static FudgeMsg ToFudgeMsg<TKey, TValue>(IFudgeSerializer s, IDictionary<TKey, TValue> dict, Func<TKey, object> keyMsgGen = null, Func<TValue, object> valueMsgGen = null)
        {
            var fudgeSerializer = new FudgeSerializer(s.Context);

            keyMsgGen = keyMsgGen ?? (k=>fudgeSerializer.SerializeToMsg(k));
            valueMsgGen = valueMsgGen ?? (v => fudgeSerializer.SerializeToMsg(v));

            FudgeMsg valuesMessage = new FudgeMsg(s.Context);
            foreach (var value in dict)
            {
                valuesMessage.Add(1, keyMsgGen(value.Key));
                valuesMessage.Add(2, valueMsgGen(value.Value));
            }
            return valuesMessage;
        }
    }
}
