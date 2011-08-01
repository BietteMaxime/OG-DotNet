//-----------------------------------------------------------------------
// <copyright file="ViewDefinitionCompilationFailedCall.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Engine.View.listener
{
    public class ViewDefinitionCompilationFailedCall
    {
        private readonly DateTimeOffset _valuationTime;
        private readonly JavaException _exception;

        public ViewDefinitionCompilationFailedCall(DateTimeOffset valuationTime, JavaException exception)
        {
            _valuationTime = valuationTime;
            _exception = exception;
        }

        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }

        public JavaException Exception
        {
            get { return _exception; }
        }

        public static ViewDefinitionCompilationFailedCall FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ViewDefinitionCompilationFailedCall(ffc.GetValue<DateTimeOffset>("valuationTime"), deserializer.FromField<JavaException>(ffc.GetByName("exception")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
