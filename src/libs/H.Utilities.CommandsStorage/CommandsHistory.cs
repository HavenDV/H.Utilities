using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace H.Utilities;

public class CommandsHistory
{
    #region Properties

    public const string FileName = "Commands-History.json";

    public string CompanyName { get; }
    public AppDataFile AppDataFile { get; }

    #endregion

    #region Constructors

    public CommandsHistory(string companyName)
    {
        CompanyName = companyName;
        AppDataFile = new AppDataFile(CompanyName, FileName);
    }

    #endregion

    #region Public methods

    public void Add(string command)
    {
        var history = Load();
        history.Add(command);

        AppDataFile.FileData = JsonConvert.SerializeObject(history, Formatting.Indented);
    }

    public List<string> Load()
    {
        var text = AppDataFile.FileData;
        if (text == null || string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        var list = JsonConvert.DeserializeObject<List<string>>(text)
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .ToList();
        list.Reverse();

        return list;
    }

    public void Clear()
    {
        AppDataFile.Clear();
    }

    #endregion
}