using System;
using System.IO;
using Microsoft.Win32;

namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class Startup
    {
        private static string KeyName { get; } = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="autoStart"></param>
        /// <param name="arguments"></param>
        public static void Set(string fileName, bool autoStart, string? arguments = null)
        {
            var name = ToName(fileName);

            using (var mainKey = Registry.CurrentUser)
            using (var key = mainKey.CreateSubKey(KeyName))
            {
                if (key == null)
                {
                    throw new InvalidOperationException($"Cannot open/create reg key: {KeyName}");
                }

                if (autoStart)
                {
                    var value = ToValue(fileName, arguments);
                    key.SetValue(name, value);
                }
                else if (key.GetValue(name) != null)
                {
                    key.DeleteValue(name);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string? GetFilePath(string fileName)
        {
            var name = ToName(fileName);

            using var mainKey = Registry.CurrentUser;
            using var key = mainKey.OpenSubKey(KeyName);

            if (!(key?.GetValue(name) is string value))
            {
                return null;
            }

            return ToFilePath(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsStartup(string path)
        {
            var valuePath = GetFilePath(path);

            return Compare(valuePath, path);
        }

        #region Private methods

        private static string ToName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var name = Path.GetFileNameWithoutExtension(fileName);
            if (name == null)
            {
                throw new ArgumentException("File name is not correct");
            }

            return name;
        }

        private static string ToValue(string fileName, string? arguments) => $"\"{fileName}\" {arguments}".Trim();

        private static string? ToFilePath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var values = value.Trim().Split(new[] {'\"'}, StringSplitOptions.RemoveEmptyEntries);

            return values.Length > 0 ? values[0] : null;
        }

        private static bool Compare(string? first, string? second) => 
            string.Equals(first?.Trim(' ', '\"'), second?.Trim(' ', '\"'), StringComparison.OrdinalIgnoreCase);

        #endregion

    }
}