using System.Collections.Generic;

namespace RM2ExCoop.RM2C
{
    internal class Mat
    {
        readonly Dictionary<string, F3DCommand?> _mat;

        public Mat(Dictionary<byte, F3DCommand> dict)
        {
            _mat = new();

            foreach (var (key, value) in dict)
            {
                value.SetArgs();
                if (key == 0xF5)
                    _mat[key.ToString() + '7'] = value;
                _mat[key.ToString()] = value;
            }
        }

        public bool HasAttr(string key) => _mat.ContainsKey(key);

        public F3DCommand? this[string key] => _mat[key];
        public void Set(string key, F3DCommand cmd) => _mat[key] = cmd;
    }
}
