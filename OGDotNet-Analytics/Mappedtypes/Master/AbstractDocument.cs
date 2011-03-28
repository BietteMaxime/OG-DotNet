using System;

namespace OGDotNet.Mappedtypes.Master
{
    public abstract class AbstractDocument
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
    }
}
