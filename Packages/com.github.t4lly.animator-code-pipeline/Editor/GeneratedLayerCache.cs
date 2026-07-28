using System;
using System.Collections.Generic;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Lazily creates one generated layer value for each exact suffix.
    /// Requesting the same suffix again returns the same generated layer.
    /// </summary>
    internal sealed class GeneratedLayerCache<TValue> where TValue : class
    {
        private readonly Dictionary<string, TValue> _items =
            new Dictionary<string, TValue>(StringComparer.Ordinal);

        internal int Count => _items.Count;

        internal TValue GetOrCreate(string suffix, Func<string, TValue> factory)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                throw new ArgumentException("A non-empty layer suffix is required.", nameof(suffix));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            if (_items.TryGetValue(suffix, out var existing))
                return existing;

            var created = factory(suffix);
            if (created == null)
                throw new InvalidOperationException("The generated layer factory returned null.");

            _items.Add(suffix, created);
            return created;
        }
    }
}
