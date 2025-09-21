using System.Collections.Generic;
using System.IO;

namespace server.IO;

public class IniFile(string path)
{
    private readonly Dictionary<string, Dictionary<string, string>> _iniFile = [];
    private readonly string _path = File.Exists(path) ? path : throw new FileNotFoundException();

    public void Load()
    {
        using var reader = new StreamReader(File.OpenRead(_path));
        var curHeader = "";
        while (!reader.EndOfStream)
        {
            var curLine = reader.ReadLine();

            if (string.IsNullOrEmpty(curLine))
                continue;

            if (curLine.StartsWith("["))
            {
                curHeader = curLine;
                _iniFile.Add(curHeader, []);
            }
            else
            {
                var kvp = curLine.Split("=", 2);
                if (string.IsNullOrEmpty(kvp[0]) || string.IsNullOrEmpty(kvp[1]))
                    continue;
                _iniFile[curHeader].TryAdd(kvp[0], kvp[1]);
            }
        }
    }
    public IReadOnlyDictionary<string, Dictionary<string, string>> GetDictionary() => _iniFile;
    public string GetValue(string header, string key, object defaultValue)
    {
        return !_iniFile.ContainsKey(header)
            ? (string)defaultValue
            : _iniFile[header].TryGetValue(key, out var value) ? value : (string)defaultValue;
    }

    public void SetValue(string value, string header, string key)
    {
        if (!_iniFile.ContainsKey(header))
            _iniFile.Add(header, []);

        _iniFile[header][key] = value;
    }

    public void Save()
    {
        using var writer = new StreamWriter(File.OpenWrite(_path));
        foreach (var kvp in _iniFile)
        {
            writer.WriteLine(kvp.Key);
            foreach (var kvp2 in kvp.Value)
                writer.WriteLine(kvp2.Key + "=" + kvp2.Value);

            writer.WriteLine();
        }
    }
}