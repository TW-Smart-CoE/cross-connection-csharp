using System.Collections.Generic;

namespace CConn
{
    public class ConfigProps
    {
        private readonly IDictionary<string, object> dict = new Dictionary<string, object>();

        public static ConfigProps Create()
        {
            return new ConfigProps();
        }

        public ConfigProps Put<T>(string key, T value)
        {
            dict[key] = value;

            return this;
        }

        public string Get(string key, string defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                return data.ToString();
            }
            else
            {
                return defaultValue;
            }
        }

        public bool Get(string key, bool defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                var strValue = data.ToString();
                if (bool.TryParse(strValue, out bool result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public int Get(string key, int defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                var strValue = data.ToString();
                if (int.TryParse(strValue, out int result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public uint Get(string key, uint defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                var strValue = data.ToString();
                if (uint.TryParse(strValue, out uint result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public short Get(string key, short defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                var strValue = data.ToString();
                if (short.TryParse(strValue, out short result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue; 
            }
        }

        public ushort Get(string key, ushort defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                var strValue = data.ToString();
                if (ushort.TryParse(strValue, out ushort result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public long Get(string key, long defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                var strValue = data.ToString();
                if (long.TryParse(strValue, out long result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            } 
        }

        public ulong Get(string key, ulong defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                var strValue = data.ToString();
                if (ulong.TryParse(strValue, out ulong result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            } 
        }

        public float Get(string key, float defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                var strValue = data.ToString();
                if (float.TryParse(strValue, out float result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            } 
        }

        public double Get(string key, double defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                var strValue = data.ToString();
                if (double.TryParse(strValue, out double result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }  
        }

        public byte[] GetBytes(string key, byte[] defaultValue)
        {
            if (dict.TryGetValue(key, out object data))
            {
                return data as byte[];
            }
            else
            {
                return defaultValue;
            } 
        }
    }
}

