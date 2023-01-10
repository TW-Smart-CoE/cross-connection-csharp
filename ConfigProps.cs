using System.Collections.Generic;

namespace CConn
{
    public class ConfigProps
    {
        private IDictionary<string, string> dict = new Dictionary<string, string>();

        public static ConfigProps create()
        {
            return new ConfigProps();
        }

        public ConfigProps Put<T>(string key, T value)
        {
            dict[key] = value.ToString();

            return this;
        }

        public string Get(string key, string defaultValue)
        {
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                return strValue;
            }
            else
            {
                return defaultValue;
            }
        }

        public bool Get(string key, bool defaultValue)
        {
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                bool data;
                if (bool.TryParse(strValue, out data))
                {
                    return data;
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
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                int data;
                if (int.TryParse(strValue, out data))
                {
                    return data;
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
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                uint data;
                if (uint.TryParse(strValue, out data))
                {
                    return data;
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
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                short data;
                if (short.TryParse(strValue, out data))
                {
                    return data;
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
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                ushort data;
                if (ushort.TryParse(strValue, out data))
                {
                    return data;
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
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                long data;
                if (long.TryParse(strValue, out data))
                {
                    return data;
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
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                ulong data;
                if (ulong.TryParse(strValue, out data))
                {
                    return data;
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
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                float data;
                if (float.TryParse(strValue, out data))
                {
                    return data;
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
            string strValue = "";
            if (dict.TryGetValue(key, out strValue))
            {
                double data;
                if (double.TryParse(strValue, out data))
                {
                    return data;
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
    }
}

