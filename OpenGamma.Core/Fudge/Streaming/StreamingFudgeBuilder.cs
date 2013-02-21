// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamingFudgeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Fudge.Encodings;

using OpenGamma.Master;
using OpenGamma.Model;
using OpenGamma.Util.FudgeMsg;

namespace OpenGamma.Fudge.Streaming
{
    /// <summary>
    /// Registry of builders capable of working directly from the root of an IFudgeStreamReader
    /// These can be faster, and have much lower memory requirements than the standard builders
    /// </summary>
    class StreamingFudgeBuilder
    {
        static readonly ConcurrentDictionary<Type, IStreamingFudgeBuilder> Builders = new ConcurrentDictionary<Type, IStreamingFudgeBuilder>(new IStreamingFudgeBuilder[]
                                                                                {
                                                                                    new DependencyGraphStreamingBuilder()
                                                                                }.ToDictionary(b => b.Type));

        private static readonly Dictionary<Type, Type> GenericBuilders = new Dictionary<Type, Type>
                                                                             {
                                                                                 {typeof(SearchResult<>), typeof(SearchResultStreamingBuilder<>) }, 
                                                                                 {typeof(FudgeListWrapper<>), typeof(FudgeListWrapperStreamingBuilder<>) } // TODO generate keys
                                                                             };

        private readonly OpenGammaFudgeContext _context;

        public StreamingFudgeBuilder(OpenGammaFudgeContext context)
        {
            _context = context;
        }

        public bool TryDeserialize<T>(Stream stream, out T t)
        {
            IStreamingFudgeBuilder builder;
            var type = typeof(T);
            if (Builders.TryGetValue(type, out builder))
            {
                t = Build<T>(builder, stream);
                return true;
            }

            Type genericBuilderType;

            // Should normally fall at the first hurdle
            if (type.IsGenericType
                && GenericBuilders.TryGetValue(type.GetGenericTypeDefinition(), out genericBuilderType))
            {
                // Add to dictionary to avoid repeated reflection
                builder = Builders.GetOrAdd(type, _ => CreateBuilder(type, genericBuilderType));
                t = Build<T>(builder, stream);
                return true;
            }

            t = default(T);
            return false;
        }

        private static IStreamingFudgeBuilder CreateBuilder(Type type, Type genericBuilderType)
        {
            var closedType = genericBuilderType.MakeGenericType(type.GetGenericArguments());
            var constructorInfo = closedType.GetConstructor(new Type[] {});
            return (IStreamingFudgeBuilder) constructorInfo.Invoke(null);
        }

        private T Build<T>(IStreamingFudgeBuilder builder, Stream stream)
        {
            return builder.Deserialize<T>(_context, new FudgeEncodedStreamReader(_context, stream), _context.GetSerializationTypeMap());
        }
    }
}