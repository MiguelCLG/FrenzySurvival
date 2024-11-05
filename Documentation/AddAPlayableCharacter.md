# How to add a playable character

## NEEDS

- Sprite frames with all animations
- List of abilities this character should have

## What to do

- Create a `Character Selection Resource`
  - Fill in:
    - Name
    - Portrait
    - `AbilityResourceDatabase` (Create this first.)
    - `PlayerResource` (Create this first.)
- Create a `AbilityResourceDatabase`
  - Fill in:
    - Dictionary of abilities with `<Name, Scene>`
- Create a `PlayerResource`
  - Fill in:
  - RaceResource (Optional)
  - Portrait (might deprecate and do it automatically)
  - AbilityResourceDatabase (might deprecate and do it automatically)
  - Character Stats
    - Fill in:
      - Base stats
      - Animated Frames (SpriteFrames) for the new character
  - Mobs Only Group (ignore)
