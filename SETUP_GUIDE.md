# Story-Driven Game with Progression Tracking - Setup Guide

## Overview
This system provides a complete framework for a story-driven horror game with player progression tracking. It includes:

- **GameManager**: Central hub for game state and events
- **ProgressionSystem**: Tracks player level, experience, and achievements
- **StoryManager**: Manages chapters, dialogue, and narrative progression
- **PlayerDataManager**: Handles save/load functionality
- **ProgressionUI**: Displays player progress and stats
- **StoryUI**: Displays story, dialogue, and objectives

## Setup Instructions

### 1. Scene Setup
1. Open your main game scene
2. Create an empty GameObject called `GameManager`
3. Add the `GameManager` script to it
4. Mark it as a prefab that persists across scenes using `DontDestroyOnLoad`

### 2. UI Canvas Setup
1. Create a Canvas in your scene
2. Add a Panel for progression UI with the following child elements:
   - TextMeshProUGUI for Level
   - Image for Experience Bar (use slider or image)
   - TextMeshProUGUI for Chapter Progress
   - Image for Chapter Progress Bar
   - TextMeshProUGUI for Achievement Count
   - TextMeshProUGUI for Objective Count

3. Add another Panel for Story UI with:
   - TextMeshProUGUI for Chapter Title
   - TextMeshProUGUI for Chapter Description
   - TextMeshProUGUI for Dialogue Speaker
   - TextMeshProUGUI for Dialogue Text
   - TextMeshProUGUI for Objectives
   - Button to Advance Dialogue

### 3. Script Integration

#### Add ProgressionUI to Canvas:
1. Select your progression UI panel
2. Add the `ProgressionUI` script
3. Assign the TextMeshProUGUI and Image components to the appropriate fields

#### Add StoryUI to Canvas:
1. Select your story UI panel
2. Add the `StoryUI` script
3. Assign the TextMeshProUGUI components to the appropriate fields
4. Assign the advance dialogue button's onClick event to `StoryUI.AdvanceDialogue()`

### 4. Customizing Your Story

Edit **StoryManager.cs** in the `InitializeStory()` method to add your chapters:

```csharp
Chapter chapter1 = new Chapter(0, "Chapter 1: Title", "Description");
chapter1.AddDialogue(new DialogueNode("Speaker", "Dialogue text", DialogueNode.DialogueType.NPC));
chapter1.AddObjective("objective_id", "Objective description");
chapters.Add(chapter1);
```

### 5. Integrating with Your Game Logic

Use the `ProgressionExample.cs` script as a reference. Common use cases:

**Award XP for defeating enemies:**
```csharp
GameManager.Instance.GainExperience(50);
```

**Complete an objective:**
```csharp
GameManager.Instance.CompleteObjective("explore_area");
```

**Unlock an achievement:**
```csharp
GameManager.Instance.UnlockAchievement("first_kill", "First Blood", "Defeat your first enemy");
```

**Advance the story:**
```csharp
GameManager.Instance.AdvanceChapter(nextChapterIndex);
```

**End the game:**
```csharp
GameManager.Instance.EndGame(victory: true); // or false
```

## Features

### Player Progression
- **Leveling System**: Exponential experience curve (BASE_EXP_FOR_LEVEL * level²)
- **Objectives**: Track completion of specific tasks
- **Achievements**: Unlock special accomplishments
- **Auto-Save**: Progress is saved automatically on quit

### Story Management
- **Chapters**: Organize narrative into chapters with titles and descriptions
- **Dialogue**: Support for Player, NPC, and Narrator dialogue types
- **Objectives**: Display chapter-specific goals
- **Progress Tracking**: Monitor story completion percentage

### Save/Load System
- Save file location: `Application.persistentDataPath/playerdata.json`
- Automatic save on game quit
- Manual save: `GameManager.Instance.SaveProgress()`
- Load on startup: `PlayerDataManager.LoadPlayerData()`

## Event System

Subscribe to important game events:

```csharp
GameManager.Instance.OnProgressionUpdated += () => { 
    Debug.Log("Progress updated!");
};

GameManager.Instance.OnGameStateChanged += () => {
    Debug.Log("Game state changed!");
};

ProgressionSystem progressionSystem = GameManager.Instance.GetProgressionSystem();
progressionSystem.OnLevelUp += (level) => {
    Debug.Log($"Leveled up to {level}!");
};

progressionSystem.OnAchievementUnlock += (achievement) => {
    Debug.Log($"Achievement unlocked: {achievement.title}");
};
```

## Example Workflow

1. Game starts → GameManager creates instances of all systems
2. Player plays and completes objectives → `GainExperience()` called
3. Player levels up → `OnLevelUp` event fires
4. UI updates automatically via event subscriptions
5. Player completes chapter objectives → Story progresses
6. Player quits game → Progress automatically saved
7. Player returns → Game loads saved progress automatically

## Debugging

Use the `ProgressionExample` script's `PrintPlayerProgress()` method or:

```csharp
GameManager.Instance.GetProgressionSystem().DebugPrintProgress();
```

This will output:
- Current level
- Total XP
- Level progress percentage
- Objective count
- Achievement count

## Customization Tips

1. **Modify XP Requirements**: Change `BASE_EXP_FOR_LEVEL` in `ProgressionSystem.cs`
2. **Add More Chapters**: Follow the pattern in `StoryManager.InitializeStory()`
3. **Customize UI**: Replace TextMeshProUGUI with your preferred UI elements
4. **Add More Stat Tracking**: Extend `PlayerData` class with additional fields
5. **Custom Save System**: Modify `PlayerDataManager` to use your preferred format (XML, binary, etc.)

## Next Steps

1. Set up the scene and UI as described above
2. Customize the story in StoryManager
3. Integrate progression calls into your existing enemy AI and game logic
4. Test save/load functionality
5. Add sound effects and animations for level up and achievement events
