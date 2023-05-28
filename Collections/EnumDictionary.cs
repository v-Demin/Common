using System;
using System.Collections.Generic;
using System.Linq;

namespace Submodules.Common.Collections
{
    public class EnumDictionary<TEnum, TValue> where TEnum : Enum
    {
        public readonly Dictionary<TEnum, TValue> Dictionary = new ();
        public readonly int Length;

        public EnumDictionary()
        {
            var enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
            Length = enumValues.Length;

            foreach (var value in enumValues)
            {
                Dictionary.Add(value, default);
            }
        }

        public TValue this[TEnum enumIndex]
        {
            get => Dictionary[enumIndex];
            set => Dictionary[enumIndex] = value;
        }

        public override string ToString()
        {
            return Dictionary.ToList()
                .Select(pair => $"{pair.Key.ToString()}: {pair.Value.ToString()}")
                .Aggregate((cur, next) => $"{cur}, {next}");
        }
    }
}