//-----------------------------------------------------------------------
// <copyright file="StreamingFudgeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

#define TEST_STREAMING_BUILDER_SPEED

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fudge.Encodings;
using OGDotNet.Model;
using OGDotNet.Model.Resources;

namespace OGDotNet.Builders.Streaming
{
    /// <summary>
    /// Registry of builders capable of working directly from the root of an IFudgeStreamReader
    /// These can be faster, and have much lower memory requirements than the standard builders
    /// </summary>
    class StreamingFudgeBuilder
    {
        static readonly Dictionary<Type, IStreamingFudgeBuilder> Builders = new IStreamingFudgeBuilder[]
                                                                                {
                                                                                    new SecuritiesResponseStreamingBuilder(),
                                                                                    new DependencyGraphStreamingBuilder()
                                                                                }.ToDictionary(b => b.Type);

        private readonly OpenGammaFudgeContext _context;

        public StreamingFudgeBuilder(OpenGammaFudgeContext context)
        {
            _context = context;
        }

        public bool TryDeserialize<T>(Stream stream, out T t)
        {
            IStreamingFudgeBuilder builder;
            if (Builders.TryGetValue(typeof(T), out builder))
            {
                t = builder.Deserialize<T>(_context, new FudgeEncodedStreamReader(_context, stream));
                return true;
            }
            t = default(T);
            return false;
        }
    }
}