using UnityEditor;
using UnityEngine;

namespace MCPForUnity.Editor
{
    [InitializeOnLoad]
    public static class McpBootstrapper
    {
        static McpBootstrapper()
        {
            Debug.Log("McpBootstrapper: Starting stdio for CI...");
            McpCiBoot.StartStdioForCi();
        }
    }
}
