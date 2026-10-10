# Witch Knight: concept and scope

> Title: **Witch Knight** (repo: `witch-knight`). A full-overhaul Hollow Knight mod that plays like a Hades 2 run, starring an original witch-bug heroine.

## 1. Ground rules for a public release

- **Only original work goes in the repo.** No art, audio, text or code extracted from Hades 2 or Hollow Knight.
- **No Supergiant names.** No "Melinoë", "Hecate", "Descura" or other Hades 2 item names. Greek myth itself is public domain, but Supergiant's versions of it are not, so we use our own patrons (below).
- **Patch the game at runtime.** The mod loads through the Hollow Knight Modding API, and players need their own copy of Hollow Knight.
- **License:** MIT for code, CC BY-NC-SA 4.0 for our original art (suggested; easy to change).

## 2. The heroine

| | |
|---|---|
| Species | A moth, from a lost lunar branch of Hallownest's Moth Tribe |
| Role | Witch-princess raised in the dark beneath the Abyss, trained in hex-craft |
| Goal | Strike down the being that stole her moon (stands in for Chronos as a time-themed antagonist, rewritten as an original Hallownest threat) |
| Look | Small, cloaked, pale wing-cloak with crescent markings, glowing eyes, a staff taller than she is |
| Palette | Silver-white, deep violet, a single green-gold accent (moonlight on a moth wing) |
| Feel | Agile and floaty: a short wing-dash in place of the Knight's dash, a slow-fall glide |

## 3. Systems map: Hades 2 → Hollow Knight

| Hades 2 idea | Hexmoth version | Hollow Knight base it replaces |
|---|---|---|
| Nocturnal Arms (6 weapons) | **Six witch-tools** swapped at a bench: Staff, Twin Pins, Lantern, Moon-Axe, Skull-Charm, Mantle | The Nail and its upgrades |
| Attack / Special | Nail strike = Attack, Nail Art input = Special | Nail strike, Nail Arts |
| Omega moves (hold to charge, costs Magick) | Hold Attack or Special to charge an empowered version | Nail Art charge |
| Magick | **Moonlight** meter, regenerates over time and on hits | Soul |
| Cast (binding circle) | **Hex Circle**: drops a ring that roots enemies | Vengeful Spirit slot |
| Boons from Olympians | **Favours** from five original patrons, offered in rooms, stacking per run | New system |
| Arcana cards | **Arcana charms**: permanent passive cards, equipped with a point budget | Charms and notches |
| Incantations at the cauldron | **Rites** at a base camp, unlocking permanent world changes | Shops and NPC upgrades |
| Run-based structure | **Runs** on two routes (see below); death returns you to camp | Persistent map (removed) |
| Hexes (Selene) | **Moon Hexes**: one ultimate spell per run, charged by spending Moonlight | Desolate Dive / Howling Wraiths slots |

### Original patrons (instead of Olympians)

Each patron has a theme, a colour and a set of Favours:

1. **The Pale Tide**: water and knockback.
2. **The Ember Choir**: fire and damage over time.
3. **The Silk Matron**: webs, slows and traps.
4. **The Hollow Bell**: sound, stun and area pulses.
5. **The Last Bloom**: healing, thorns and growth.

## 4. Scope by milestone

1. **M1, playable heroine:** a custom skin, Moonlight in place of Soul, the Staff only (Attack, Special, Omega), and the wing-dash.
2. **M2, run loop:** the base camp, the Descent route built from existing Hollow Knight rooms, Favours from 2 patrons, death returning to camp.
3. **M2.5, second route:** the Ascent route, with its own bosses and a surface-only threat.
4. **M3, full arsenal:** all six witch-tools, all five patrons, Arcana charms, Hex Circle and Moon Hexes.
5. **M4, content:** reworked bosses, a final antagonist, original areas, and Rites.

Each milestone is a public GitHub release.

## 5. Tech stack

- **Hollow Knight 1.5.x** (current Steam version), with the **Hollow Knight Modding API**.
- **C# / .NET Framework 4.7.2**, matching the game's Unity Mono runtime.
- **Helper libraries:** Satchel (FSM and texture helpers), and CustomKnight for skin loading in M1.
- **Mod installers for players:** Lumafly or Scarab, through the community mod list once stable.
- **Build:** GitHub Actions builds the DLL against reference assemblies and attaches it to releases.

## 6. Decisions

- **Runs only.** There is no open-world story mode. The game is run-based, like Hades, and the persistent Hollow Knight map is not playable on its own.
- **Two routes from camp.** Each run, the heroine picks a route:
  - **Descent (down):** from Dirtmouth through the Crossroads, Greenpath, Fungal Wastes and Deepnest to the Abyss.
  - **Ascent (up):** from the Crystal Peak mines through the City of Tears rooftops to the Howling Cliffs and the Hive's peak.
  Each route has its own room pools, bosses and Favour odds. Rooms are cut from the existing Hollow Knight areas.
- **Keyboard only.** No controller support is planned.
- **Text barks only.** The heroine and NPCs speak through short text lines. There is no voice acting.

## 7. Room flow in a run

Each region is about 8 to 12 rooms long. After clearing a room, the player picks the next door, and each door shows its reward (a Favour, Moonstones, healing, a shop or a mini-boss). Each region ends in a boss, and a full run is 4 regions.

## 8. Bestiary: Hallownest reborn as Greek myth

Every Hollow Knight creature becomes a creature or figure from Greek myth. The myths are public domain, so we use our own designs and never Supergiant's versions. Each reskinned creature keeps its Hollow Knight movement and attack pattern as a base, then gets one new move that fits its myth.

### Common enemies

| Hollow Knight | Witch Knight | New twist |
|---|---|---|
| Wandering Husk, Husk Hornhead | **Hoplite Husk**: a bug in a bronze helmet with a spear | Raises its shield and blocks frontal hits until it attacks |
| Husk Guard, Husk Sentry | **Phalanx Guard** | Fights in pairs, locking shields side by side |
| Great Husk Sentry | **Spartan Champion** | Throws its spear across the room |
| Vengefly | **Harpy Fly** | Steals a dropped Moonstone and flees |
| Gruzzer | **Stymphalian Chick** | Sheds bronze feathers when hit |
| Aspid Hunter, Primal Aspid | **Hydra Hatchling** | Grows a second head and fires twice if not killed quickly |
| Crawlid, Tiktik | **Bronze Crawler**: a tiny wind-up automaton | Explodes into gears on death |
| Mosscreep, Mosskin | **Dryad Sprout** | Roots the heroine briefly on contact |
| Mistake, Folly | **Shade** | Passes through walls |
| Crystal Hunter, Crystal Crawler | **Cyclops Miner** | Fires a single-eye beam |
| Deepnest spiders, Weavers | **Arachne's Brood** | Leaves webs that slow her |
| Hiveling, Hive Soldier | **Melissae**: bee nymphs | Heal each other with honey |

### Bosses

| Hollow Knight | Witch Knight | New twist |
|---|---|---|
| False Knight | **The Minotaur**: a maggot in a bull-skull helmet with a double axe | Charges horns-first, and the arena becomes a short labyrinth in phase 2 |
| Gruz Mother | **Stymphalian Mother** | Her bronze feathers rain from the ceiling |
| Brooding Mawlek | **Chimera**: lion, goat and serpent heads | Each head has its own attack, and the goat head heals the others |
| Hornet (Greenpath) | **The Amazon** | Throws a chakram-like bronze disc that returns |
| Mantis Lords | **The Gorgon Sisters**: three sisters | Petrifying gaze, so she must look away by facing the other direction |
| Soul Master | **The Fallen Oracle of Delphi** | Teleports between prophecy circles that mark where the next attack lands |
| Dung Defender | **The Augean Keeper**, from Heracles' labours | Rolls dung boulders, and a river channel floods the arena |
| Broken Vessel | **The Hydra** | Cut heads regrow as two unless burned with a Moonlight spell |
| Crystal Guardian | **Talos**: the bronze giant | Sweeps a heat-ray, and is weak at his ankle |
| Watcher Knights | **Argus Hundred-Eyes** | A group fight where each knight carries one of Argus's eyes |
| Uumuu | **Scylla** | Grabs with tentacles from the fog lake |
| Nosk | **Empusa**: the shapeshifter | Mimics the heroine's silhouette before attacking |
| Hive Knight | **The Hive Warden of the Melissae** | Summons bee nymphs between charges |
| Traitor Lord | **Lycaon**: a wolf-cursed king | Lunges, and calls a howling shockwave |
| The Hollow Knight, Radiance | **The Moon-Thief** (original final antagonist) | Built last, in M4 |

### How a reskin is built

1. **Art:** a rig in the same style as the heroine, exported to sprite sheets.
2. **Swap:** at runtime the mod hides the original sprite and plays our frames, timed to the enemy's existing animation clips.
3. **Twist:** a small change to the enemy's PlayMaker behaviour, or a Witch Knight component added to it.

Order: the Descent route first, in this order: Hoplite Husk, Harpy Fly, Bronze Crawler, then the Minotaur as the first boss.
