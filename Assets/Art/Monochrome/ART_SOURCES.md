# KeySlaught monochrome art sources

Date: 2026-09-22

All shipped sprites in this folder are original project-specific work. No Asset Store pack or online game-art asset was used.

## Visual-development source

`GeneratedSources/Milestone6_StyleAtlas.png` was created with OpenAI's built-in image-generation tool as an AI-assisted visual-development sheet. It established the requested minimalist grayscale, gothic-academic silhouette language for roads, ground, turrets, the player, Library, enemy cards, brain pickup, torches, rocks, clouds, trees, water, and mountains.

Prompt summary: create a coherent minimalist monochrome pixel-art atlas for a portrait typing/tower-defense game, including six strictly orthogonal road connections, symmetric ground variants, three simple academic turret silhouettes, a hooded player, a pyramid university, square enemy cards, pickups, terrain, and UI icons; grayscale only, transparent background, no text, logos, or external assets.

## Shipped sprite construction

`AgentScripts/BuildMilestone6Scene.cs` authors the production PNGs from deterministic pixel primitives and a five-color grayscale palette. This keeps every tile, character frame, turret, icon, and pickup consistent and reproducible:

- gameplay tiles, actors, turrets, obstacles, pickups, and UI icons: exactly 128 x 128 pixels;
- Library: exactly 256 x 256 pixels for a 2 x 2 tile footprint;
- four ground variants;
- six path connections: horizontal, vertical, left-top, top-right, left-bottom, and bottom-right;
- four water variants and four mountain variants;
- Point filtering, 128 pixels per unit, no mipmaps, and uncompressed Unity imports.

The generated atlas is retained for transparent provenance and design review. The game does not slice or ship sprites directly from that atlas.
