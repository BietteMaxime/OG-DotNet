namespace OGDotNet
{
    public interface IUniqueIdentifiable
    {

        /**
         * Gets the unique identifier for this item.
         * 
         * @return the unique identifier, may be null
         */
        UniqueIdentifier UniqueId { get; }

    }
}