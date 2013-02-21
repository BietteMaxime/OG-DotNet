// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using OpenGamma.Id;

namespace OpenGamma.Master
{
    public abstract class AbstractDocument : IUniqueIdentifiable
    {
        private readonly DateTimeOffset _versionFromInstant;
        private readonly DateTimeOffset _versionToInstant;
        private readonly DateTimeOffset _correctionFromInstant;
        private readonly DateTimeOffset _correctionToInstant;

        protected AbstractDocument() : this(default(DateTimeOffset), default(DateTimeOffset), default(DateTimeOffset), default(DateTimeOffset))
        {
        }

        protected AbstractDocument(DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant)
        {
            _versionFromInstant = versionFromInstant;
            _versionToInstant = versionToInstant;
            _correctionFromInstant = correctionFromInstant;
            _correctionToInstant = correctionToInstant;
        }

        public DateTimeOffset VersionFromInstant
        {
            get { return _versionFromInstant; }
        }

        public DateTimeOffset VersionToInstant
        {
            get { return _versionToInstant; }
        }

        public DateTimeOffset CorrectionFromInstant
        {
            get { return _correctionFromInstant; }
        }

        public DateTimeOffset CorrectionToInstant
        {
            get { return _correctionToInstant; }
        }

        public abstract UniqueId UniqueId { get; set; }
    }
}
