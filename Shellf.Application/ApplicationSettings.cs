namespace Shellf.Application;

using System.Collections.Generic;

/**
** TODOs
** - Use shellf icon
** - Kill all processes on exit
** - Checkmark/track running by id or by name
** - Stop if running / Start if stopped
*/

using System.Text.Json.Serialization;

public class ApplicationSettings
{
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("commands")]
    public List<CommandGroup> Commands { get; set; }
}

public class CommandGroup
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("items")]
    public List<CommandItem> Items { get; set; }
}

public class CommandItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("exe")]
    public string Exe { get; set; }

    [JsonPropertyName("startCommand")]
    public string StartCommand { get; set; }

    [JsonPropertyName("stopCommand")]
    public string StopCommand { get; set; }

    [JsonPropertyName("workingDirectory")]
    public string WorkingDirectory { get; set; }
}


