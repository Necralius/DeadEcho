using System.Collections.Generic;

namespace Project.Core.Services
{
    public sealed class ScenePayload
    {
        private readonly IReadOnlyDictionary<string, string> _values;

        public ScenePayload(string entryKind, IReadOnlyDictionary<string, string> values = null)
        {
            EntryKind = string.IsNullOrWhiteSpace(entryKind) ? "Default" : entryKind;
            _values = values ?? new Dictionary<string, string>();
        }

        public string EntryKind { get; }
        public IReadOnlyDictionary<string, string> Values => _values;

        public bool TryGetValue(string key, out string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }

            return _values.TryGetValue(key, out value);
        }
    }
}
