using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Submodules.Common.Architecture.Providers
{
    public class AbstractEventProviderGroup
    {
        private readonly List<AbstractEventProvider> _eventProviders = new List<AbstractEventProvider>();

        [Inject]
        public AbstractEventProviderGroup(IInstantiator container)
        {
        }
    
        public T GetProvider<T>() where T : AbstractEventProvider
        {
            var provider = _eventProviders.FirstOrDefault(provider => provider is T) as T;
            return provider ?? throw new ArgumentException($"Провайдер для типа {typeof(T)} не найден");
        }
    }
}
