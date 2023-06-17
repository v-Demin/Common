using Common.Data;

namespace Submodules.Common.Architecture.Providers
{
    public abstract class BaseDataProvider<D> : AbstractDataProvider where D : AbstractData
    {
        public D Data { get; private set; }
        public override void CollectData() => Data = CollectDataInner();
        protected abstract D CollectDataInner();
    }
}
