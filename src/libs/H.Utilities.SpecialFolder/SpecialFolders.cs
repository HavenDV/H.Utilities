using System;

namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class AppDataFolder : SpecialFolder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="needToCombinedPathStrings"></param>
        public AppDataFolder(params string[] needToCombinedPathStrings) :
            base(Environment.SpecialFolder.ApplicationData, needToCombinedPathStrings)
        {
        }
    }
}
