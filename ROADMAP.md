# ChatterShade Roadmap

This roadmap tracks planned improvements for ChatterShade. The priority
is to keep the current plugin stable, test changes carefully, and build
new features in manageable steps.

Planned features are goals, not promises of a particular release date.
Priorities may change as compatibility testing and community feedback.

## Phase 0 --- Stabilization and Compatibility

**Goal:** Keep the current functionality reliable before expanding the
plugin.

-   Continue testing standard NPC dialogue, multi-speaker cutscenes, and
    multi-line dialogue.
-   Test Dark/System Dialogue styling and NPC name customization.
-   Verify that original text colors and backgrounds are restored when
    dialogue closes, the UI is reused, or ChatterShade is disabled or
    unloaded.
-   Check window resizing, UI scale changes, and compatibility with
    other UI modifications.
-   Address feedback and issues reported during plugin review and
    community testing.
-   Keep known limitations documented, including matching speaker
    profiles by displayed name and support for different dialogue
    interfaces.

**Completion checks** - Existing features pass regression testing. -
Settings and speaker profiles persist after restarting the game. -
Original game appearance is restored correctly through normal dialogue
transitions and plugin lifecycle changes. - Known limitations and
confirmed compatibility are documented.

## Phase 1 --- Complete UI and Configuration Improvements

**Goal:** Redesign the settings interface and make appearance
customization easier to understand and use.

### Complete UI Redesign

-   Give ChatterShade its own visual identity using the a unique style and direction.
-   Use persistent left navigation with the workspace and preview on the
    right.
-   Organize the interface into Appearance, Speakers, Presets, and
    Settings workspaces.
-   Use a restrained midnight-navy, aether-blue, and crystal-gold
    palette while keeping text and controls readable.
-   Preserve the existing configuration properties, dialogue hooks, and
    restoration logic during the initial redesign.
-   Keep the existing Speaker Profile Editor working while the new
    interface is introduced.

### Live Preview

-   Add a simulated dialogue preview to the settings interface.
-   Preview speaker names, dialogue text, multi-line text, and supported
    background appearances.
-   Update the preview immediately as appearance controls change.
-   Allow users to preview Standard Dialogue and Dark/System Dialogue.
-   Keep preview rendering separate from the actual in-game dialogue
    nodes.

### Undo and Section-Specific Resets

-   Add section-specific resets for Standard Dialogue, Dark/System
    Dialogue, global NPC name customization, and individual speaker
    profiles where appropriate.
-   Add an Undo workflow so users can recover from recent changes.
-   Define and document the default values for each reset.
-   Ensure resetting one section does not unintentionally change
    unrelated settings.

### Named Visual Presets

-   Allow users to save, rename, duplicate, apply, and delete named
    visual presets.
-   Define which settings each preset contains.
-   Decide whether presets apply to all appearance settings or to
    selected sections.
-   Keep speaker profiles separate from visual presets unless a future
    design explicitly calls for combining them.

**Completion checks** - Existing settings remain accessible and
functional after the UI redesign. - Live Preview updates without
changing actual dialogue nodes. - Resets and Undo restore the expected
values without affecting unrelated sections. - Named presets save and
load correctly and do not corrupt existing configuration.

## Phase 2 --- Speaker Profile Improvements

**Goal:** Make individual speaker profiles easier to organize and
improve how speakers are identified.

### Enhanced Speaker-Profile Management

-   Improve profile organization and navigation.
-   Add alphabetical sorting and useful filtering options.
-   Consider profile duplication, enabled/disabled filters, and optional
    notes or categories where they add value.
-   Keep selection stable when profiles are sorted, filtered, edited, or
    deleted.
-   Preserve existing profiles when configuration changes are
    introduced.

### Improved Speaker Matching

-   Investigate whether a reliable NPC or actor identifier is available
    in supported dialogue contexts.
-   Compare normal NPC dialogue with cutscenes and other supported
    contexts before changing the matching method.
-   Retain displayed-name matching as a fallback where appropriate.
-   Only add more specific matching when the identifier and its behavior
    have been verified.
-   Continue documenting limitations involving duplicate names and
    unnamed speakers.

**Completion checks** - Adding, editing, searching, sorting, enabling,
disabling, duplicating, and deleting profiles work correctly. - Profiles
persist after reload. - Any new matching method is tested across
supported dialogue contexts and does not break the existing fallback.

## Phase 3 --- Import, Export, Backups, and Diagnostics

**Goal:** Improve configuration portability, recovery, and
troubleshooting.

### Import/Export and Backups

-   Support exporting settings and speaker profiles to a portable file.
-   Consider exporting selected visual presets or speaker profiles
    separately.
-   Validate imported data, including malformed files, unsupported
    schema versions, invalid colors, and duplicate entries.
-   Preserve the current configuration if an import fails.
-   Create a recovery path before replacing existing settings.
-   Keep future configuration changes migration-safe.

### Diagnostic Mode

-   Add an optional diagnostic mode to help identify compatibility and
    styling problems.
-   Show useful information such as the recognized dialogue interface,
    relevant nodes found, speaker-profile matches, styling decisions,
    and restoration problems.
-   Avoid expensive per-frame logging and avoid collecting unnecessary
    dialogue content.
-   Make it possible to disable diagnostics when they are not needed.

**Completion checks** - Exported settings can be imported again without
unintended changes. - Invalid imports fail safely and leave the current
configuration intact. - Diagnostics help investigate missing nodes,
unmatched speakers, UI reuse, dialogue closure, and restoration failures
without causing performance or stability problems.

## Phase 4 --- Appearance Expansion

**Goal:** Add more appearance options after the existing styling
behavior is stable.

### Additional Background Styles

-   Investigate additional built-in background textures and styles.
-   Consider more background appearance options, including tint,
    opacity, and corner treatment where supported.
-   Check texture loading, resource cleanup, sizing, clipping, and
    repeated switching between original and custom backgrounds.
-   Ensure new styles work at different UI scales.

### Advanced Text Styling

-   Investigate which text properties can be changed safely in the
    supported FFXIV UI nodes.
-   Explore options such as font size, weight, outline, or shadow only
    where the game UI and Dalamud APIs support them reliably.
-   Test multi-line text, long text, localization, scaling, clipping,
    and restoration.
-   Do not promise or expose styling options until their behavior has
    been verified.

**Completion checks** - Additional styles do not leak resources or
interfere with original backgrounds. - Text styling changes remain
readable and do not introduce clipping or restoration issues. -
Unsupported options are excluded or clearly documented.

## Phase 5 --- Additional Dialogue Support and Quick Controls

**Goal:** Extend ChatterShade carefully beyond the dialogue interfaces
it already supports.

### Additional Dialogue-Type Support

-   Identify other dialogue interfaces that may benefit from
    customization.
-   Investigate their addon names, node structures, active states,
    speaker and text representation, and restoration behavior.
-   Do not assume that every dialogue window uses the same node layout
    as the current Talk addon.
-   Test interface transitions, addon reuse, plugin shutdown, and
    compatibility with other UI modifications.
-   Document which dialogue types are supported and which are not.

### Commands and Quick Controls

-   Consider commands for opening specific workspaces, toggling
    supported styling, and switching visual presets.
-   Consider hotkeys later if they can be implemented without
    conflicting with gameplay or existing input behavior.
-   Build on the preset and configuration workflows rather than
    introducing a separate way to store appearance settings.
-   Test repeated commands, dialogue transitions, and plugin shutdown
    for stale styling or state.

**Completion checks** - Each additional dialogue type is individually
verified and documented. - Quick controls use the same underlying
settings and preset behavior as the main interface. - Commands and
optional shortcuts do not leave dialogue styling in an incorrect state.

