using System;
using System.Collections.Generic;
using Common.Data;

namespace Common.Architecture.Providers.Data
{
    public class DataContainerGroup
    {
        private Dictionary<Type, AbstractData> DataDictionary = new ();

        public void StoreData<D>(D data) where D : AbstractData
        {
            if (DataDictionary.ContainsKey(data.GetType()))
            {
                DataDictionary[data.GetType()] = data;
                return;
            }
        
            DataDictionary.Add(data.GetType(), data);
        }

        public D GetData<D>() where D : AbstractData
        {
            return (D)DataDictionary[typeof(D)];
        }
    }
}
