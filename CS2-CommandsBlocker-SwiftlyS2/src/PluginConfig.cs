namespace CS2_CommandsBlocker_SwiftlyS2;

public class PluginConfig
{
    public List<string> BlockedCommands { get; set; } = ["meta list", "meta"];
    public string AllowedPermission { get; set; } = "admin.root";
    
    public bool ContainsBlockType { get; set; } = true;
    public bool SendMessage { get; set; } = false;
}