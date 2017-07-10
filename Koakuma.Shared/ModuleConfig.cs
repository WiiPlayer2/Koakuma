using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Koakuma.Shared
{
    public class ModuleConfig
    {
        #region Private Fields

        private static Regex lineRegex = new Regex(@"^(?<key>[^\. =]+(\.[^\. =]+)*) *= *(?<value>.*)$");

        #endregion Private Fields

        #region Private Fields

        private static Dictionary<Type, Func<string, object>> conversions;

        private Dictionary<string, string> values;

        #endregion Private Fields

        #region Public Constructors

        static ModuleConfig()
        {
            conversions = new Dictionary<Type, Func<string, object>>();

            Register(o => o);
            Register(o => int.Parse(o, CultureInfo.InvariantCulture));
            Register(o => bool.Parse(o));
        }

        public ModuleConfig(string file)
        {
            Filepath = file;
            values = new Dictionary<string, string>();
            Reload();
        }

        #endregion Public Constructors

        #region Public Properties

        public string Filepath { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public static void Register<T>(Func<string, T> conversion)
        {
            if (!conversions.ContainsKey(typeof(T)))
            {
                conversions[typeof(T)] = str => conversion(str);
            }
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (values.ContainsKey(key))
            {
                var val = values[key];
                if (conversions.ContainsKey(typeof(T)))
                {
                    try
                    {
                        return (T)conversions[typeof(T)](val);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
                else
                {
                    throw new NotSupportedException($"The conversion to {typeof(T)} is not supported.");
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public void Reload()
        {
            try
            {
                var lines = File.ReadAllLines(Filepath);
                foreach (var l in lines)
                {
                    var m = lineRegex.Match(l);
                    if (m.Success)
                    {
                        values[m.Groups["key"].Value] = m.Groups["value"].Value;
                    }
                }
            }
            catch { }
        }

        #endregion Public Methods
    }
}