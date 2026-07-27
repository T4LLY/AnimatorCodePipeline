using System;
using System.Collections.Generic;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Lazily creates one generated layer value for each normalized suffix.
    /// Distinct source suffixes that normalize to the same AAC layer suffix are rejected.
    /// </summary>
    internal sealed class GeneratedLayerCache<TValue> where TValue : class
    {
        private sealed class Entry
        {
            internal Entry(string sourceSuffix, TValue value)
            {
                SourceSuffix = sourceSuffix;
                Value = value;
            }

            internal string SourceSuffix { get; }
            internal TValue Value { get; }
        }

        private readonly Dictionary<string, Entry> _items =
            new Dictionary<string, Entry>(StringComparer.Ordinal);

        internal int Count => _items.Count;

        internal TValue GetOrCreate(string suffix, Func<string, TValue> factory)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                throw new ArgumentException("A non-empty layer suffix is required.", nameof(suffix));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var normalizedSuffix = NormalizeSuffix(suffix);

            if (_items.TryGetValue(normalizedSuffix, out var existing))
            {
                if (!string.Equals(existing.SourceSuffix, suffix, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Animator Code Pipeline layer suffix collision: '{existing.SourceSuffix}' and '{suffix}' " +
                        $"both map to AAC layer suffix '{normalizedSuffix}'. Use suffixes that remain distinct " +
                        "after '.' is replaced with '_'.");
                }

                return existing.Value;
            }

            var created = factory(normalizedSuffix);
            if (created == null)
                throw new InvalidOperationException("The generated layer factory returned null.");

            _items.Add(normalizedSuffix, new Entry(suffix, created));
            return created;
        }

        internal static string NormalizeSuffix(string suffix)
        {
            if (suffix == null) throw new ArgumentNullException(nameof(suffix));
            return suffix.Replace('.', '_');
        }
    }
}
