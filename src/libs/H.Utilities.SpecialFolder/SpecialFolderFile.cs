using System;
using System.IO;
using System.Linq;

namespace H.Utilities;

/// <summary>
/// 
/// </summary>
public class SpecialFolderFile : SpecialFolder
{
    #region Properties
        
    /// <summary>
    /// 
    /// </summary>
    public string FileName { get; }
        
    /// <summary>
    /// 
    /// </summary>
    public string FullPath => Path.Combine(Folder, FileName);

    /// <summary>
    /// 
    /// </summary>
    public string? FileData
    {
        get => File.Exists(FullPath) ? File.ReadAllText(FullPath) : null;
        set => File.WriteAllText(FullPath, value);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specialFolder"></param>
    /// <param name="needToCombinedPathStrings"></param>
    public SpecialFolderFile(Environment.SpecialFolder specialFolder, params string[] needToCombinedPathStrings) : 
        base(specialFolder, needToCombinedPathStrings.Reverse().Skip(1).Reverse().ToArray())
    {
        FileName = needToCombinedPathStrings.Any()
            ? needToCombinedPathStrings[needToCombinedPathStrings.Length - 1]
            : throw new ArgumentException(@"Need one or more values", nameof(needToCombinedPathStrings));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        FileData = string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Delete()
    {
        File.Delete(FullPath);
    }

    #endregion
}