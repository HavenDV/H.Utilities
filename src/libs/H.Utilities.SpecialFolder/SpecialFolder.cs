using System;
using System.Collections.Generic;
using System.IO;

namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class SpecialFolder
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public Environment.SpecialFolder EnvironmentSpecialFolder { get; }

        /// <summary>
        /// 
        /// </summary>
        public string SubPath { get; }

        /// <summary>
        /// 
        /// </summary>
        public string SpecialFolderPath => Environment.GetFolderPath(EnvironmentSpecialFolder);

        /// <summary>
        /// 
        /// </summary>
        public string Folder => Directory.CreateDirectory(
                Path.Combine(SpecialFolderPath, SubPath))
            .FullName;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Files => Directory.EnumerateFiles(Folder, "*.*");

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="specialFolder"></param>
        /// <param name="needToCombinedPathStrings"></param>
        public SpecialFolder(Environment.SpecialFolder specialFolder, params string[] needToCombinedPathStrings)
        {
            EnvironmentSpecialFolder = specialFolder;
            SubPath = Path.Combine(needToCombinedPathStrings);
        }

        #endregion
    }
}