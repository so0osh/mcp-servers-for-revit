# Getting Real-World Terrain (Height Map) into Revit by Lat/Lon

> **Wiki / How-To** — companion to the `create_toposolid` and `get_project_location` MCP tools.

## TL;DR

Revit has **no built-in feature** that automatically downloads a real-world height map (DEM) from a
latitude/longitude pair. The Revit API (2027 included) only lets you *construct* terrain from data you
already have — points, contour curves, or an imported CAD/point file (`Toposolid.Create`,
`Toposolid.CreateFromTopographySurface`, "Create from Import Instance").

Getting the actual elevation data for a real site requires one of four pipelines, summarized below.

| # | Pipeline | Automatable? | Needs external service? | Native Autodesk? |
|---|----------|--------------|--------------------------|-------------------|
| 1 | Civil 3D / InfraWorks bridge | Partial (desktop workflow) | No (uses Autodesk data) | Yes (AEC Collection) |
| 2 | Third-party plugins (Elk/Rhino.Inside, Topo Setter, etc.) | Yes | Yes (Mapbox/SRTM APIs) | No |
| 3 | Manual DEM download + conversion | Yes (scriptable) | Yes (USGS/OpenTopography/Google) | No |
| 4 | **Autodesk Forma** | Semi (cloud sync, UI-driven) | No (Autodesk's own geodata) | Yes (AEC Collection) |

Use `get_project_location` (this MCP server) to read the project's `SiteLocation`/`ProjectPosition`
once terrain/geolocation has been established by any of the pipelines below — it gives you the
lat/lon anchor and the internal↔shared coordinate transform needed to place new geometry correctly.

---

## Option 1 — Civil 3D / InfraWorks Bridge

**What it is:** The "official" heavy-civil pipeline. Civil 3D can pull real elevation data (USGS,
SRTM, or a surveyed/point-cloud source) and generate a proper civil **surface**. That surface is then
exported as contours or a point grid and linked/imported into Revit.

**Steps:**
1. In Civil 3D, create or import a **Surface** for your site (from survey points, LiDAR, or a
   downloaded DEM raster converted to a TIN surface).
2. Export contours as a DWG (polylines with elevation) — or export the surface as a point file (`.txt`/`.csv`
   with X, Y, Z columns).
3. In Revit, link/import that DWG or point file, then use **Toposolid → Create from Import Instance**
   to generate the Toposolid from the imported contour/point data.
4. Set the project's `SiteLocation` (Latitude/Longitude/Elevation) to match the real-world coordinate
   your Civil 3D surface was built at, so future geometry aligns correctly.

**Pros:** Highest data fidelity, industry-standard for large/civil sites, keeps a single source of truth
in Civil 3D for regrading/earthworks.
**Cons:** Requires a Civil 3D license and manual desktop steps; not something your MCP server can drive
end-to-end (Civil 3D has no equivalent headless API exposed here).

---

## Option 2 — Third-Party Plugins

**What it is:** Community/commercial add-ins that fetch elevation tiles for a given lat/lon bounding box
and drop ready-made terrain geometry directly into your modeling tool.

**Common tools:**
- **Elk** (Grasshopper plugin, via **Rhino.Inside.Revit**) — queries Mapbox/SRTM elevation tiles for a
  given lat/lon box, builds a mesh/point grid, and can push it into Revit through Rhino.Inside.
- **"Topo Setter" / similar Revit add-ins** — import a heightmap image or raster and convert grayscale
  values to elevation points at a chosen scale.
- **GIS import add-ins** — read `.tif`/`.asc` raster DEM files and rasterize them into a Toposolid-ready
  point grid inside Revit.

**Steps (generic):**
1. Install the plugin (Elk, Rhino.Inside.Revit, or a Revit GIS add-in).
2. Provide the site's lat/lon bounding box (or pin a location on a map inside the tool).
3. The plugin calls its backing elevation service (Mapbox Terrain-RGB, SRTM, etc.) and returns a
   point grid or mesh.
4. Convert/export that grid to Revit-native geometry (Toposolid points or an import instance).

**Pros:** Fully automatable, scriptable (Grasshopper definitions can be run headlessly), good for
generative/parametric site studies.
**Cons:** Depends on a third-party API key and rate limits (Mapbox etc.); data resolution varies by
provider and region; not an Autodesk-supported pipeline.

---

## Option 3 — Manual DEM Download + Conversion (build-it-yourself)

**What it is:** Download a Digital Elevation Model for the site yourself and convert it into whatever
your `create_toposolid` tool expects (a boundary + interior points, or a point grid).

**Data sources:**
- **USGS 3DEP** (US, ~1m–30m resolution, free)
- **OpenTopography** (global, various free DEMs: SRTM 30m, ALOS, etc.)
- **Google Maps Elevation API** (point-sampling, paid beyond free tier)
- **Copernicus/ESA DEM** (global, free, coarser resolution)

**Steps:**
1. Determine the site's bounding box from `get_project_location` (or a manually specified lat/lon
   rectangle).
2. Download the DEM raster (GeoTIFF) covering that box from one of the sources above.
3. Sample the raster at a chosen grid spacing (e.g. every 5m) to get an array of
   `(lat, lon, elevation)` triples.
4. Convert each `(lat, lon)` pair to internal Revit XYZ using the **inverse** of the geodesic +
   `ProjectPosition` transform described in `get_project_location`'s docstring:
   - Project lat/lon to local meters (equirectangular or UTM projection centered on the Survey Point).
   - Rotate by `-ProjectPosition.Angle` and offset by the project's `EastWest`/`NorthSouth`/`Elevation`
     to land in internal (project) coordinates (feet).
5. Feed the resulting point grid into `create_toposolid` (as boundary + optional inner points) to
   generate the terrain.

**Pros:** No dependency on a plugin or Autodesk cloud service; fully scriptable/headless — this is the
approach your own MCP server could automate end-to-end (server-side or agent-side script + this MCP's
`create_toposolid`/`get_project_location` tools).
**Cons:** You own the projection math and data-quality tradeoffs; free DEM sources can be coarse
(30m posting) for small/urban sites.

> **This is the option that best fits "the MCP server does it all."** If you want, a follow-up tool
> (`create_toposolid_from_heightmap` or similar) could accept a lat/lon grid + elevation array and do
> the coordinate conversion + `create_toposolid` call internally.

---

## Option 4 — Autodesk Forma

**What it is:** Forma (formerly Spacemaker) is Autodesk's cloud site-analysis product. Drop a pin at a
real-world address/lat-lon inside Forma and it **automatically fetches** terrain elevation, surrounding
context buildings, roads, and other GIS layers for that location from its own geospatial datasets — no
manual DEM sourcing needed.

**Steps:**
1. Open Forma (web app), search/pin the site's real-world location.
2. Forma auto-populates terrain + context (buildings, roads) for that area.
3. In Revit (2024–2027), open the **Forma extension** (Add-Ins ribbon) and connect to the same Forma
   project.
4. Use **Import from Forma** to bring the terrain into Revit — it arrives as a `Toposolid`
   (Revit 2024+) / `TopographySurface` (older), already geolocated (sets `SiteLocation` for you).
5. Optionally push Revit massing back up to Forma for sun/wind/noise/density analysis.

**Pros:** Fully native Autodesk workflow, zero manual data wrangling, geolocation handled
automatically, bidirectional (Revit ↔ Forma).
**Cons:** Requires an **AEC Collection** subscription with Forma entitlement; sync is UI-driven through
Autodesk's cloud — **there is no public/scriptable API surface for this from `RevitAPI.dll`**, so it
cannot be automated from this MCP server. It's a manual, one-time (or occasional-refresh) step a human
performs in Revit's UI before your MCP tools take over for programmatic modeling.

---

## Recommendation Matrix

| Need | Best option |
|------|-------------|
| Enterprise/large civil site, existing Civil 3D workflow | **1 — Civil 3D/InfraWorks** |
| Quick parametric/generative massing study, scriptable via Grasshopper | **2 — Third-party plugin (Elk)** |
| Fully headless pipeline your MCP server can run end-to-end | **3 — Manual DEM + `create_toposolid`** |
| One-off real project, want native Autodesk quality with zero data wrangling | **4 — Forma** |

## Related MCP Tools

- `get_project_location` — read `SiteLocation`/`ProjectPosition` (lat/lon, elevation, and the
  internal↔shared coordinate transform) once geolocation has been set by any pipeline above.
- `create_toposolid` — construct the Toposolid element from boundary loops + points (Revit 2027+).
