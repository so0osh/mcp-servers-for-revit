import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withRevitConnection } from "../utils/ConnectionManager.js";

export function registerGetProjectLocationTool(server: McpServer) {
  server.tool(
    "get_project_location",
    "Get the active project's geolocation and coordinate transform. Returns the site's " +
      "latitude/longitude (decimal degrees) and elevation (mm) from the Survey Point's " +
      "geographic anchor, plus the eastWest/northSouth/elevation offset (mm) and angle " +
      "(radians, project-to-true-north rotation) that map internal project coordinates " +
      "to shared/survey coordinates. Use this to convert real-world lat/lon into the " +
      "internal x/y/z coordinates expected by other creation tools.",
    {},
    async (args, extra) => {
      try {
        const response = await withRevitConnection(async (revitClient) => {
          return await revitClient.sendCommand("get_project_location", {});
        });

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(response, null, 2),
            },
          ],
        };
      } catch (error) {
        return {
          content: [
            {
              type: "text",
              text: `Get project location failed: ${
                error instanceof Error ? error.message : String(error)
              }`,
            },
          ],
        };
      }
    }
  );
}
