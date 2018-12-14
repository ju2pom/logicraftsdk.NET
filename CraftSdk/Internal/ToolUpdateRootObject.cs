using System.Collections.Generic;
namespace CraftSdk.Internal
{
    internal class ToolUpdateRootObject
    {
        public string message_type { get; set; }
        public string session_id { get; set; }
        public string show_overlay { get; set; }
        public string tool_id { get; set; }
        public List<ToolOption> tool_options { get; set; }
        public string play_task { get; set; }
    }
}
