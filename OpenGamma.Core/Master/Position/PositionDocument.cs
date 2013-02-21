// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Master.Position
{
    [FudgeSurrogate(typeof(PositionDocumentBuilder))]
    public class PositionDocument : AbstractDocument
    {
        private readonly ManageablePosition _position;

        public PositionDocument(ManageablePosition position)
            : base(default(DateTimeOffset), default(DateTimeOffset), default(DateTimeOffset), default(DateTimeOffset))
        {
            _position = position;
        }

        public PositionDocument(DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant, UniqueId uniqueId, ManageablePosition position)
            : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
            UniqueId = uniqueId;
            _position = position;
        }

        public override UniqueId UniqueId { get; set; }

        public ManageablePosition Position
        {
            get { return _position; }
        }
    }
}
