//-----------------------------------------------------------------------
// <copyright file="WindowLocationPersister.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace OGDotNet.WPFUtils
{
    public static class WindowLocationPersister
    {
        public static void InitAndPersistPosition(Window window, SettingsBase settings)
        {
            InitAndPersistPosition(window, settings, w => w.GetType().FullName);
        }

        private static void InitAndPersistPosition(Window window, SettingsBase settings, Func<Window, string> idGenerator)
        {
            const string settingName = "WindowLocationPersisterData";
            string id = idGenerator(window);
            { // Init data
                Dictionary<string, object> data = GetData((string)settings[settingName], id);
                InitWindow(window, data);
            }

            window.Closing += delegate
            {
                Dictionary<string, object> data = GetData((string)settings[settingName], id);
                PickleWindow(window, data);
                settings[settingName] = SaveData(data, id);
            };
        }

        private static void PickleWindow(Window window, Dictionary<string, object> data)
        {
            foreach (var propName in new[] { "Top", "Left", "Width", "Height", "WindowState" })
            {
                data[propName] = window.GetType().GetProperty(propName).GetGetMethod().Invoke(window, new object[] { });
            }
        }

        private static void InitWindow(Window window, Dictionary<string, object> data)
        {
            foreach (var property in data)
            {
                window.GetType().GetProperty(property.Key).GetSetMethod().Invoke(window, new[] { property.Value });
            }

            if (!Double.IsNaN(window.Top))
                window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private static string SaveData(Dictionary<string, object> data, string id)
        {
            return string.Join(",", data.Select(kvp => GetKeyPrefix(id) + kvp.Key + '|' + kvp.Value));
        }

        private static Dictionary<string, object> GetData(string stringEncoded, string id)
        {
            if (string.IsNullOrEmpty(stringEncoded))
                return new Dictionary<string, object>();

            string keyPrefix = GetKeyPrefix(id);

            return stringEncoded.Split(',').Select(s => s.Split('|'))
                .Where(p => p[0].StartsWith(keyPrefix)).
                ToDictionary(p => p[0].Substring(keyPrefix.Length), p => UnPickle(p[1]));
        }

        private static object UnPickle(string o)
        {
            double d;
            if (Double.TryParse(o, out d))
                return d;
            WindowState ws;
            if (Enum.TryParse(o, out ws))
                return ws;
            throw new ArgumentException();
        }

        private static string GetKeyPrefix(string id)
        {
            return GetKey(id, string.Empty);
        }

        private static string GetKey(string id, string property)
        {
            return string.Format("Window:{0}:{1}", id, property);
        }
    }
}
