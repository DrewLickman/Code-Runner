# Unity MCP Setup Guide

Unity MCP has been successfully configured! Follow these steps to complete the setup.

## Installation Status

✅ **Python**: Installed (3.13.1)  
✅ **uv**: Installed (via winget)  
✅ **Unity Package**: Added to `Packages/manifest.json`

## Step 1: Install the Package in Unity

1. **Open Unity Editor** with your Code-Runner project
2. Unity will automatically detect the new package in `manifest.json` and start downloading it
3. Wait for the package to finish importing (check the bottom-right progress bar)
4. If Unity doesn't auto-import, go to `Window > Package Manager` and click "Refresh"

## Step 2: Start the MCP Server

1. In Unity Editor, go to **`Window > MCP for Unity`**
2. Make sure the **Transport** dropdown is set to `HTTP Local` (default)
3. The **HTTP URL** should be `http://localhost:8080` (default)
4. Click **"Start Server"**
   - Unity will spawn a new terminal window running the server
   - **Keep this terminal window open** while you work
   - You should see "Session Active" status in the Unity window

## Step 3: Configure Cursor

### Option A: Auto-Configuration (Recommended)

1. In Unity, go to `Window > MCP for Unity`
2. Select **"Cursor"** from the Client/IDE dropdown
3. Click **"Configure"** button
4. Look for a green status indicator 🟢 and "Connected ✓"

### Option B: Manual Configuration

If auto-configuration fails, manually edit Cursor's MCP configuration:

1. **Find Cursor's MCP config file** (typically in your user settings)
2. **Add the Unity MCP server** configuration:

```json
{
  "mcpServers": {
    "unityMCP": {
      "url": "http://localhost:8080/mcp"
    }
  }
}
```

**Note**: Make sure the URL matches what's shown in Unity's MCP window (include `/mcp` at the end).

## Step 4: Verify Connection

1. **Restart Cursor** to load the new MCP configuration
2. **Make sure Unity is open** with the MCP server running
3. **Test the connection** by asking Cursor to:
   - "List all GameObjects in the current scene"
   - "Create a red cube in the scene"
   - "Show me the Unity project info"

## Available Features

Once connected, you can use natural language to:

- ✅ **Manage Assets**: Import, create, modify, delete, search assets
- ✅ **Control Editor**: Play mode, active tool, tags, layers
- ✅ **Manage GameObjects**: Create, modify, delete, find, duplicate, move
- ✅ **Manage Components**: Add, remove, set properties on components
- ✅ **Manage Materials**: Create, set properties, colors, assign to renderers
- ✅ **Manage Prefabs**: Open/close stage, save, create from GameObject
- ✅ **Manage Scenes**: Load, save, create, get hierarchy, screenshot
- ✅ **Manage Scripts**: Create, read, delete, apply edits
- ✅ **Batch Operations**: Execute multiple commands at once (10-100x faster!)

## Performance Tip: Use `batch_execute`

When performing multiple operations, use `batch_execute` instead of calling tools one-by-one:

```
❌ Slow: Create 5 cubes → 5 separate manage_gameobject calls
✅ Fast: Create 5 cubes → 1 batch_execute call with 5 commands
```

## Troubleshooting

### Unity Bridge Not Running/Connecting
- Ensure Unity Editor is open
- Check the status window: `Window > MCP for Unity`
- Restart Unity if needed

### MCP Client Not Connecting
- **Verify Server is Running**: Check Unity's MCP window shows "Session Active"
- **Check URL**: Make sure the URL in Cursor config matches Unity (include `/mcp`)
- **Restart Cursor**: After changing MCP config, restart Cursor
- **Check Terminal**: The server terminal window must stay open

### uv Not Found
- If Unity shows "uv Not Found", you may need to restart your terminal/PowerShell
- Or manually specify the path: `C:\Users\YOUR_USERNAME\AppData\Local\Microsoft\WinGet\Links\uv.exe`
- Use the "Choose `uv` Install Location" button in Unity's MCP window

### Package Not Installing
- Make sure Unity Editor is closed when editing `manifest.json`
- Open Unity and check `Window > Package Manager` for errors
- Try removing the package entry, saving, then adding it back

## Next Steps

1. Open Unity and let the package import
2. Start the MCP server from `Window > MCP for Unity`
3. Configure Cursor (auto or manual)
4. Start using AI-powered Unity development!

## Documentation

- **GitHub**: https://github.com/CoplayDev/unity-mcp
- **Discord**: Join the community for help and updates
- **Asset Store**: Available on Unity Asset Store

## Example Prompts

Try these once everything is set up:

- "Create a 3D player controller with jump and movement"
- "Create a tic-tac-toe game in 3D"
- "Create a cool shader and apply it to a cube"
- "Find all GameObjects with a Rigidbody component"
- "Add a light to the scene and make it rotate"

Happy coding! 🚀
