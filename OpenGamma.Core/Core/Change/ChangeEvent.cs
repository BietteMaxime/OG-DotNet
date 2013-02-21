// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeEvent.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;
using Fudge.Types;

using OpenGamma.Fudge;
using OpenGamma.Id;
using OpenGamma.Time;

namespace OpenGamma.Core.Change
{
    public class ChangeEvent
    {
        private readonly ChangeType _type;
        private readonly UniqueId _beforeId;

        private readonly UniqueId _afterId;
        private readonly Instant _versionInstant;

        public ChangeEvent(ChangeType type, UniqueId beforeId, UniqueId afterId, Instant versionInstant)
        {
            _type = type;
            _beforeId = beforeId;
            _afterId = afterId;
            _versionInstant = versionInstant;
        }

        public ChangeType Type
        {
            get { return _type; }
        }

        public UniqueId BeforeId
        {
            get { return _beforeId; }
        }

        public UniqueId AfterId
        {
            get { return _afterId; }
        }

        public Instant VersionInstant
        {
            get { return _versionInstant; }
        }

        public static ChangeEvent FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var changeType = EnumBuilder<ChangeType>.Parse(ffc.GetMessage("type").GetString(1));
            var beforeField = ffc.GetString("beforeId");
            var beforeId = beforeField == null ? null : UniqueId.Parse(beforeField);
            var afterField = ffc.GetString("afterId");
            var afterId = afterField == null ? null : UniqueId.Parse(afterField);
            var versionField = ffc.GetByName("versionInstant");
            var versionInstant = (FudgeDateTime) versionField.Value;
            return new ChangeEvent(changeType, 
                beforeId, 
                afterId, 
                new Instant(versionInstant));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
