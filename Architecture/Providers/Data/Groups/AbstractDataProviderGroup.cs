using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Submodules.Common.Architecture.Providers
{
    public class AbstractDataProviderGroup
    {
        private readonly List<AbstractDataProvider> _eventProviders = new ();

        [Inject]
        public AbstractDataProviderGroup(IInstantiator container)
        {
        }
    
        public T GetProvider<T>() where T : AbstractDataProvider
        {
            var provider = _eventProviders.FirstOrDefault(provider => provider is T) as T;
            return provider ?? throw new ArgumentException($"Провайдер типа {typeof(T)} не найден");
        }
    }
}
