# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 2D game project called "Dungeon Maker" (던전 메이커) where players design dungeons as a demon lord to defeat heroes and collect wealth. It's a learning project designed to study Unity game development step-by-step with AI-generated code.

## Common Development Commands

### Unity-specific operations
- To create scripts: Create `.cs` files in `Assets/Scripts/` directory
- To create prefabs: Reference them in scripts or save them in `Assets/Prefabs/`
- To modify scenes: Edit `Assets/Scenes/SampleScene.unity` (main scene)

### Testing
- Unity testing is done through the Unity Editor's Play mode
- No command-line test runner is configured

## High-Level Architecture

### Project Structure
- **Assets/**: Main development directory for all game assets
  - `Scenes/`: Contains Unity scene files (currently only SampleScene.unity)
  - `Scripts/`: Should contain all C# game scripts (to be created)
  - `Prefabs/`: Should contain reusable game object prefabs (to be created)
  - `Materials/`: Should contain materials and shaders (to be created)
  - `Sprites/`: Should contain 2D sprites and textures (to be created)

### Game Systems (Planned)
1. **Dungeon Building System**: Grid-based room placement with entrance, combat rooms, and treasure rooms
2. **Monster Management**: Purchase, placement, and combat systems for monsters
3. **Hero AI**: Automated pathfinding and combat behavior for invading heroes
4. **Combat System**: Turn-based battles between heroes and monsters
5. **Economy System**: Currency management for building rooms and buying monsters
6. **UI Systems**: Funds display, shop interface, monster placement panel

### Unity Configuration
- Unity version: 2023.x (uses Unity 2023 based on project structure)
- Render pipeline: Universal Render Pipeline (URP)
- 2D project with tilemap support enabled
- Key packages: 2D features, Visual Scripting, Timeline, UGUI

## Implementation Guidelines

When implementing features:
1. Create scripts in appropriate subdirectories under `Assets/Scripts/`
2. Follow Unity naming conventions (PascalCase for classes, camelCase for methods)
3. Use Unity's built-in systems (MonoBehaviour, ScriptableObjects, etc.)
4. Implement UI using Unity's UI system (Canvas, UI elements)
5. Use Unity's 2D physics for collision detection
6. Implement grid system using Unity's Tilemap or custom grid logic

## Key Implementation Tasks

Based on the README checklist:
- Fund management UI and logic
- Dungeon room grid system with placement rules
- Monster shop and inventory system
- Monster placement mechanics
- Hero spawning and pathfinding
- Turn-based combat system
- Victory/defeat conditions