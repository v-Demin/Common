using Zenject;

namespace Submodules.Common.Architecture.Providers
{
    public abstract class BaseSceneProviderService<D, E> : AbstractSceneProviderService where D : AbstractDataProvider where E : AbstractEventProvider
    {
        [Inject] private AbstractDataProviderGroup DataProviderGroup;
        [Inject] private AbstractEventProviderGroup EventProviderGroup;
        
        public D Data { get; private set; }
        public E Event { get; private set; }

        public BaseSceneProviderService()
        {
            Data = DataProviderGroup.GetProvider<D>();
            Event = EventProviderGroup.GetProvider<E>();
        }
    }
}
