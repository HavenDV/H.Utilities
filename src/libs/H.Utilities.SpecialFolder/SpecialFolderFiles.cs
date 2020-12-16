using System;

namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class AppDataFile : SpecialFolderFile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="needToCombinedPathStrings"></param>
        public AppDataFile(params string[] needToCombinedPathStrings) : 
            base(Environment.SpecialFolder.ApplicationData, needToCombinedPathStrings)
        {
        }
    }
}
