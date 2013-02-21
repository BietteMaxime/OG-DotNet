// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractLink.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;

namespace OpenGamma.Master
{
    public abstract class AbstractLink<T>
    {
        protected AbstractLink()
        {
            ExternalId = ExternalIdBundle.Empty();
        }

        public ObjectId ObjectId { get; set; }
        public ExternalIdBundle ExternalId { get; set; }
        public T Target { get; set; }
    }
}