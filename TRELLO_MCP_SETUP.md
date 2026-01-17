# Trello MCP Server Setup Guide

The Trello MCP server has been successfully installed! Follow these steps to configure it.

## Installation Status

✅ **Package Installed**: `@delorenj/mcp-server-trello` (via npm global install)

## Step 1: Get Trello API Credentials

Trello now uses the Power-Up Admin Portal to manage API keys. Follow these steps:

### Create a Power-Up (if you don't have one)

1. **Navigate to Power-Up Admin Portal**:
   - Visit: https://trello.com/power-ups/admin
   - You should see the "Your Apps" page

2. **Create a New Power-Up**:
   - Click the **"New"** button (top right of the page)
   - Fill in the required information:
     - **Name**: e.g., "Code-Runner-Trello-Integration" or "MCP Trello Server"
     - **Workspace**: Select your workspace
     - **Email**: Your email address
     - **Support Email**: Your support email
     - **Author**: Your name
     - **iframe connector URL**: You can use a placeholder like `https://example.com` (this is required but won't be used for API-only access)
   - Click **"Create"** or **"Save"**

### Get Your API Key

3. **Access the API Key**:
   - After creating the Power-Up, click on it to open its details page
   - Look for the **"API key"** tab or section
   - Click **"Generate a new API Key"** (or use "Use Existing App Key" if you have a legacy key)
   - **Copy your API Key** - keep this handy

### Generate Your Token

4. **Generate the Token** (Method 1 - Recommended):
   - In the same **"API key"** tab/section, look for a **"Token"** link or button
   - Click it to authorize the app to access your Trello account
   - Authorize the application when prompted
   - **Copy the generated token** - this is sensitive, treat it like a password!

   **OR** (Method 2 - Alternative):
   - After you have your API Key, you can also generate a token using this URL (replace `YOUR_API_KEY` with your actual API key):
     ```
     https://trello.com/1/authorize?expiration=never&name=Code-Runner-Trello-Integration&scope=read,write&response_type=token&key=YOUR_API_KEY
     ```
   - Authorize the application
   - Copy the token from the page

**Important Notes**:
- The **API Key** can be public (it's not highly sensitive)
- The **Token** is secret and grants access to your Trello data - keep it secure!
- Both are required for the MCP server to work
- If you can't find the "API key" tab, make sure you've clicked on your Power-Up from the "Your Apps" list to view its details

## Step 2: Configure for Claude Desktop

If you're using Claude Desktop, add this to your configuration file:

**Location**: `%APPDATA%\Claude\claude_desktop_config.json` (Windows)

```json
{
  "mcpServers": {
    "trello": {
      "command": "npx",
      "args": ["-y", "@delorenj/mcp-server-trello"],
      "env": {
        "TRELLO_API_KEY": "your-api-key-here",
        "TRELLO_TOKEN": "your-token-here"
      }
    }
  }
}
```

**Note**: Replace `your-api-key-here` and `your-token-here` with your actual credentials.

## Step 3: Configure for Cursor (if using MCP)

If Cursor supports MCP configuration, you may need to add similar configuration. Check Cursor's MCP documentation for the exact location and format.

## Alternative: Using Environment Variables

You can also set environment variables globally:

**Windows PowerShell**:
```powershell
[System.Environment]::SetEnvironmentVariable("TRELLO_API_KEY", "your-api-key", "User")
[System.Environment]::SetEnvironmentVariable("TRELLO_TOKEN", "your-token", "User")
```

**Windows CMD**:
```cmd
setx TRELLO_API_KEY "your-api-key"
setx TRELLO_TOKEN "your-token"
```

After setting environment variables, restart your MCP client.

## Available Features

Once configured, you'll have access to:

- ✅ List and manage Trello boards
- ✅ Create, update, and archive cards
- ✅ Manage lists and checklists
- ✅ Add comments to cards
- ✅ Attach files and images to cards
- ✅ Get comprehensive card details
- ✅ Track board activity
- ✅ Manage workspaces

## Testing the Installation

After configuration, you can test by asking your AI assistant to:
- "List my Trello boards"
- "Show me cards from [board name]"
- "Create a new card in [list name]"

## Troubleshooting

- **"Environment variables required" error**: Make sure `TRELLO_API_KEY` and `TRELLO_TOKEN` are set
- **Authentication errors**: Verify your API key and token are correct
- **Can't find API key in Power-Up Admin Portal**: 
  - Make sure you've clicked on your Power-Up from the "Your Apps" list (not just viewing the main page)
  - Look for tabs like "API key", "Settings", or "Configuration" on the Power-Up details page
  - If you don't see an API key option, try creating a new Power-Up or check if your account has the necessary permissions
- **Rate limiting**: The server automatically handles Trello's rate limits (300 req/10s per API key, 100 req/10s per token)

## Documentation

Full documentation: https://github.com/delorenj/mcp-server-trello
