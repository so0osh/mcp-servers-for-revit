import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withRevitConnection } from "../utils/ConnectionManager.js";

export function registerCreateToposolidTool(server: McpServer) {
  server.tool(
    "create_toposolid",
    "Create toposolid (site/terrain) elements from boundary loops. Requires Revit 2027 or later - " +
      "the Toposolid API replaces the legacy TopographySurface workflow. All coordinates are in millimeters (mm).",
    {
      data: z
        .array(
          z.object({
            typeId: z
              .number()
              .optional()
              .describe("The ID of the toposolid type to use. If not specified, will use the first available type."),
            boundary: z
              .object({
                outerLoop: z
                  .array(
                    z.object({
                      p0: z.object({
                        x: z.number().describe("X coordinate of start point"),
                        y: z.number().describe("Y coordinate of start point"),
                        z: z.number().describe("Z coordinate of start point"),
                      }),
                      p1: z.object({
                        x: z.number().describe("X coordinate of end point"),
                        y: z.number().describe("Y coordinate of end point"),
                        z: z.number().describe("Z coordinate of end point"),
                      }),
                    })
                  )
                  .min(3)
                  .describe("Array of line segments defining the outer boundary"),
                innerLoops: z
                  .array(
                    z.array(
                      z.object({
                        p0: z.object({
                          x: z.number().describe("X coordinate of start point"),
                          y: z.number().describe("Y coordinate of start point"),
                          z: z.number().describe("Z coordinate of start point"),
                        }),
                        p1: z.object({
                          x: z.number().describe("X coordinate of end point"),
                          y: z.number().describe("Y coordinate of end point"),
                          z: z.number().describe("Z coordinate of end point"),
                        }),
                      })
                    )
                  )
                  .optional()
                  .describe("Optional array of inner loops (holes) within the toposolid"),
              })
              .describe("Boundary definition with outer loop and optional inner loops"),
            baseLevel: z.number().describe("Base level height (mm)"),
            baseOffset: z.number().optional().describe("Offset from base level (mm)"),
          })
        )
        .describe("Array of toposolid elements to create"),
    },
    async (args, extra) => {
      const params = args;
      try {
        const response = await withRevitConnection(async (revitClient) => {
          return await revitClient.sendCommand("create_toposolid", params);
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
              text: `Create toposolid failed: ${error instanceof Error ? error.message : String(error)}`,
            },
          ],
        };
      }
    }
  );
}
