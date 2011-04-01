using OGDotNet.Model.Context.MarketDataSnapshot;

namespace OGDotNet.Mappedtypes.Master.marketdatasnapshot
{
    internal interface IUpdatableFrom<in T>
    {
        UpdateAction PrepareUpdateFrom(T newObject);
    }
}