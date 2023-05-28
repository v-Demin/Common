using System;
using System.Collections.Generic;
using System.Linq;

namespace Submodules.Common.Collections
{
    public class EnumDictionary<TEnum, TValue> where TEnum : Enum
    {
        private readonly Dictionary<TEnum, TValue> _innerDictionary = new ();
        public readonly int Length;

        public EnumDictionary()
        {
            var enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
            Length = enumValues.Length;

            foreach (var value in enumValues)
            {
                _innerDictionary.Add(value, default);
            }
        }

        public TValue this[TEnum enumIndex]
        {
            get => _innerDictionary[enumIndex];
            set => _innerDictionary[enumIndex] = value;
        }

        public override string ToString()
        {
            return _innerDictionary.ToList()
                .Select(pair => $"{pair.Key.ToString()}: {pair.Value.ToString()}")
                .Aggregate((cur, next) => $"{cur}, {next}");
        }
    }
}