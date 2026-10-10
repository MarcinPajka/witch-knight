using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modding;
using UnityEngine;

namespace WitchKnight
{
    internal enum RoomReward { Favour, Moonstones, Mending, Moonlight }

    /// <summary>
    /// The run loop. Camp is the bench in the first town. Once a route is chosen, the next
    /// exit she takes leads into the run: every exit out of a run room opens onto a random
    /// room from the route's pool, each region ends in a champion's room, and after the last
    /// champion she walks home. Clearing a room pays its reward; leaving early forfeits it.
    /// Dying sends her back to camp with her Moonstones but no Favours.
    /// </summary>
    internal class RunLoop : MonoBehaviour
    {
        public const string CampScene = "Town";
        private const string CampBench = "RestBench";
        private const string CampGate = "left1";

        private class Region
        {
            public string Name = "";
            public string[] Rooms = new string[0];
            public string Boss = "";
        }

        // Rooms are "Scene[gate]": the scene to load and the gate she walks in through.
        private static readonly Region[] descent =
        {
            new() { Name = "The Forked Ways", Boss = "Crossroads_10[left1]", Rooms = new[]
            {
                "Crossroads_05[left1]", "Crossroads_07[left1]", "Crossroads_08[left1]", "Crossroads_13[left1]",
                "Crossroads_15[left1]", "Crossroads_16[left1]", "Crossroads_19[left1]", "Crossroads_21[left1]",
                "Crossroads_33[left1]", "Crossroads_39[left1]", "Crossroads_40[left1]", "Crossroads_42[left1]",
            } },
            new() { Name = "The Verdant Hollow", Boss = "Fungus1_04[left1]", Rooms = new[]
            {
                "Fungus1_01[left1]", "Fungus1_02[left1]", "Fungus1_03[left1]", "Fungus1_07[left1]",
                "Fungus1_09[left1]", "Fungus1_10[left1]", "Fungus1_12[left1]", "Fungus1_17[left1]",
                "Fungus1_19[left1]", "Fungus1_21[left1]", "Fungus1_25[left1]", "Fungus1_29[left1]",
            } },
            new() { Name = "The Spore Marsh", Boss = "Fungus2_15[left1]", Rooms = new[]
            {
                "Fungus2_03[left1]", "Fungus2_04[left1]", "Fungus2_06[left1]", "Fungus2_07[left1]",
                "Fungus2_08[left1]", "Fungus2_09[left1]", "Fungus2_11[left1]", "Fungus2_17[left1]",
                "Fungus2_19[left1]", "Fungus2_20[left1]", "Fungus2_21[left1]", "Fungus2_33[left1]",
            } },
            new() { Name = "The Weaving Dark", Boss = "Deepnest_32[left1]", Rooms = new[]
            {
                "Deepnest_01[left1]", "Deepnest_02[left1]", "Deepnest_03[left1]", "Deepnest_14[left1]",
                "Deepnest_17[left1]", "Deepnest_26[left1]", "Deepnest_30[left1]", "Deepnest_34[left1]",
                "Deepnest_35[left1]", "Deepnest_37[left1]", "Deepnest_41[left1]", "Deepnest_43[left1]",
            } },
        };

        private static readonly Region[] ascent =
        {
            new() { Name = "The Glittering Deep", Boss = "Mines_18[left1]", Rooms = new[]
            {
                "Mines_02[left1]", "Mines_04[left1]", "Mines_05[left1]", "Mines_06[left1]",
                "Mines_07[left1]", "Mines_10[left1]", "Mines_17[left1]", "Mines_19[left1]",
                "Mines_20[left1]", "Mines_23[left1]", "Mines_29[left1]", "Mines_30[left1]",
            } },
            new() { Name = "The Weeping Rooftops", Boss = "Ruins1_24[left1]", Rooms = new[]
            {
                "Ruins1_03[left1]", "Ruins1_05b[left1]", "Ruins1_06[left1]", "Ruins1_18[left1]",
                "Ruins1_23[left1]", "Ruins1_27[left1]", "Ruins1_28[left1]", "Ruins1_30[left1]",
                "Ruins2_01_b[left1]", "Ruins2_05[left1]", "Ruins2_06[left1]", "Ruins2_07[left1]",
            } },
            new() { Name = "The Windswept Edge", Boss = "Deepnest_East_Hornet[left1]", Rooms = new[]
            {
                "Cliffs_02[left1]", "Cliffs_04[left1]", "Cliffs_05[left1]", "Cliffs_06[left1]",
                "Deepnest_East_03[left1]", "Deepnest_East_04[left1]", "Deepnest_East_06[left1]",
                "Deepnest_East_07[left1]", "Deepnest_East_09[left1]", "Deepnest_East_11[left1]",
            } },
            new() { Name = "The Honey Spire", Boss = "Hive_05[left1]", Rooms = new[]
            {
                "Hive_01[left1]", "Hive_02[left1]", "Hive_03_c[left1]", "Hive_04[left1]",
            } },
        };

        public static bool InRun { get; private set; }
        public static int RegionIndex { get; private set; }
        public static int RoomIndex { get; private set; }
        public static bool InBossRoom { get; private set; }
        public static RoomReward Reward { get; private set; }
        public static bool Cleared { get; private set; }
        public static int FoesLeft { get; private set; }
        public static int RunsWon { get; private set; }

        private static string? currentScene;
        private static readonly List<string> bag = new();
        private static readonly System.Random rng = new();
        private static RunLoop? runner;

        private static Region[] Regions => Route.Current == RouteKind.Ascent ? ascent : descent;
        private static Region CurrentRegion => Regions[Mathf.Clamp(RegionIndex, 0, Regions.Length - 1)];
        public static string RegionName => CurrentRegion.Name;
        public static int RegionCount => Regions.Length;
        public static int RoomsInRegion => Mathf.Min(Mathf.Max(1, WitchKnightMod.Settings.RoomsPerRegion), CurrentRegion.Rooms.Length);

        public static void Hook()
        {
            var go = new GameObject("WitchKnight RunLoop");
            DontDestroyOnLoad(go);
            runner = go.AddComponent<RunLoop>();

            On.GameManager.BeginSceneTransition += OnBeginSceneTransition;
            ModHooks.SceneChanged += OnSceneChanged;
            ModHooks.BeforePlayerDeadHook += OnBeforeDeath;
            ModHooks.AfterPlayerDeadHook += OnAfterDeath;
            ModHooks.AfterSavegameLoadHook += _ => { EndRun(); SendDeathsToCamp(); };
        }

        private static bool Enabled => WitchKnightMod.Settings.RunLoop;

        // Every scene change made by walking through an exit passes through here. Doors (shops,
        // houses), respawns and dream warps are left alone; so is anything outside a run room.
        private static void OnBeginSceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            if (Enabled && IsWalkingExit(info))
            {
                var from = self.sceneName;
                if (!InRun && !Route.Choosing) StartRun();
                else if (InRun && from == currentScene) NextRoom();
                else { orig(self, info); return; }

                if (InRun)
                {
                    var room = InBossRoom ? CurrentRegion.Boss : DrawRoom();
                    Split(room, out var scene, out var gate);
                    info.SceneName = scene;
                    info.EntryGateName = gate;
                    currentScene = scene;
                    BeginRoom();
                }
                else
                {
                    info.SceneName = CampScene;
                    info.EntryGateName = CampGate;
                }
            }
            orig(self, info);
        }

        private static bool IsWalkingExit(GameManager.SceneLoadInfo info)
        {
            var gate = info.EntryGateName;
            if (string.IsNullOrEmpty(gate) || gate == "dreamGate" || gate.StartsWith("door")) return false;
            return HeroController.instance != null && GameManager.instance != null && !GameManager.instance.IsNonGameplayScene();
        }

        private static void StartRun()
        {
            InRun = true;
            RegionIndex = 0;
            RoomIndex = 0;
            InBossRoom = false;
            RefillBag();
            SendDeathsToCamp();
            Barks.Say("Hex Moth", $"{Route.Name(Route.Current)}. {RegionName} first.", Barks.MothColour);
        }

        private static void NextRoom()
        {
            if (InBossRoom)
            {
                RegionIndex++;
                RoomIndex = 0;
                InBossRoom = false;
                if (RegionIndex >= Regions.Length)
                {
                    WinRun();
                    return;
                }
                RefillBag();
                Barks.Say("Hex Moth", $"{RegionName}. Further, then.", Barks.MothColour);
                return;
            }

            RoomIndex++;
            if (RoomIndex >= RoomsInRegion)
            {
                InBossRoom = true;
                Barks.Say("Hex Moth", "Something big is breathing behind this wall.", Barks.MothColour);
            }
        }

        private static void RefillBag()
        {
            bag.Clear();
            bag.AddRange(CurrentRegion.Rooms.OrderBy(_ => rng.Next()));
        }

        private static string DrawRoom()
        {
            if (bag.Count == 0) RefillBag();
            // Never walk straight back into the room she just left.
            var i = bag.Count > 1 && bag[0].StartsWith((currentScene ?? "?") + "[") ? 1 : 0;
            var room = bag[i];
            bag.RemoveAt(i);
            return room;
        }

        private static void Split(string room, out string scene, out string gate)
        {
            var open = room.IndexOf('[');
            scene = room.Substring(0, open);
            gate = room.Substring(open + 1, room.Length - open - 2);
        }

        private static void BeginRoom()
        {
            Cleared = false;
            FoesLeft = 0;
            if (InBossRoom)
            {
                Reward = RoomReward.Mending;
                return;
            }
            var r = rng.NextDouble();
            Reward = r < 0.4 ? RoomReward.Favour : r < 0.7 ? RoomReward.Moonstones : r < 0.85 ? RoomReward.Mending : RoomReward.Moonlight;
        }

        private static void WinRun()
        {
            RunsWon++;
            Barks.Say("Run", "Victory. " + Hex.Tally(), new Color(1f, 0.85f, 0.45f));
            Barks.Say("Hex Moth", "Home, for now. The moon can wait one more night.", Barks.MothColour);
            EndRun();
            Favours.Reset();
            Hex.ResetRun();
            Route.Reset();
        }

        public static void EndRun()
        {
            InRun = false;
            InBossRoom = false;
            RegionIndex = 0;
            RoomIndex = 0;
            currentScene = null;
            bag.Clear();
        }

        // Point the respawn at the camp bench so death always lands there.
        private static void SendDeathsToCamp()
        {
            if (!Enabled) return;
            var pd = PlayerData.instance;
            if (pd == null) return;
            pd.SetString("respawnScene", CampScene);
            pd.SetString("respawnMarkerName", CampBench);
            pd.SetInt("respawnType", 1);
        }

        private static void OnBeforeDeath()
        {
            SendDeathsToCamp();
        }

        // No shade in a run: she keeps her Moonstones and her full Moonlight vessel.
        private static void OnAfterDeath()
        {
            if (!Enabled) return;
            var wasInRun = InRun;
            EndRun();

            var pd = PlayerData.instance;
            if (pd == null) return;
            var pool = pd.GetInt("geoPool");
            if (pool > 0)
            {
                HeroController.instance?.AddGeo(pool);
                pd.SetInt("geoPool", 0);
            }
            pd.SetString("shadeScene", "None");
            pd.SetBool("soulLimited", false);
            GameCameras.instance?.soulOrbFSM?.SendEvent("SOUL LIMITER DOWN");
            if (wasInRun) Barks.Say("Hex Moth", "Back at camp. Choose a road, and out again.", Barks.MothColour);
        }

        private static void OnSceneChanged(string scene)
        {
            if (InRun && scene == currentScene && runner != null)
            {
                EnsureGate();
                runner.StopAllCoroutines();
                runner.StartCoroutine(WatchRoom(scene));
            }
        }

        // If a pooled gate is missing from its scene, enter through another one rather than
        // leaving her lost in the dark.
        private static void EnsureGate()
        {
            var gm = GameManager.instance;
            if (gm == null) return;
            var gates = FindObjectsOfType<TransitionPoint>();
            if (gates.Length == 0 || gates.Any(g => g.name == gm.entryGateName)) return;
            var pick = gates.FirstOrDefault(g => g.name.StartsWith("left") || g.name.StartsWith("right")) ?? gates[0];
            WitchKnightMod.Instance?.LogWarn($"Gate {gm.entryGateName} missing in {gm.sceneName}; using {pick.name}");
            gm.entryGateName = pick.name;
        }

        // A room is clear once every foe seen in it is dead or gone. Foes that wake later
        // (a champion dropping from the ceiling) join the count when they appear.
        private static IEnumerator WatchRoom(string scene)
        {
            var seen = new HashSet<HealthManager>();
            var started = Time.time;
            yield return new WaitForSeconds(1f);

            while (InRun && currentScene == scene && !Cleared)
            {
                foreach (var hm in FindObjectsOfType<HealthManager>())
                {
                    if (!hm.isDead && hm.gameObject.activeInHierarchy) seen.Add(hm);
                }
                FoesLeft = seen.Count(hm => hm != null && !hm.isDead && hm.gameObject.activeInHierarchy);
                if (FoesLeft == 0 && Time.time - started > 2.5f)
                {
                    Cleared = true;
                    while (Reward == RoomReward.Favour && Favours.Offer != null) yield return new WaitForSeconds(0.5f);
                    GrantReward(seen.Count);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        private static void GrantReward(int foes)
        {
            var hero = HeroController.instance;
            if (hero == null) return;
            switch (Reward)
            {
                case RoomReward.Favour:
                    Favours.MakeOffer();
                    break;
                case RoomReward.Moonstones:
                    var stones = 25 + 15 * RegionIndex + 4 * foes;
                    hero.AddGeo(stones);
                    Barks.Say("Hex Moth", $"{stones} Moonstones. They hum when I hold them.", Barks.MothColour);
                    break;
                case RoomReward.Mending:
                    var masks = InBossRoom ? 9 : 2;
                    hero.AddHealth(masks);
                    Barks.Say("The Last Bloom", InBossRoom ? "Rest your wings. You are whole." : "Two masks, mended.", Favours.Colour(Patron.LastBloom));
                    break;
                case RoomReward.Moonlight:
                    hero.AddMPCharge(99);
                    GameCameras.instance?.soulOrbFSM?.SendEvent("MP GAIN");
                    Barks.Say("Hex Moth", "Moonlight, brimming.", Barks.MothColour);
                    break;
            }
        }

        public static string RewardName(RoomReward r) => r switch
        {
            RoomReward.Favour => "a Favour",
            RoomReward.Moonstones => "Moonstones",
            RoomReward.Mending => "Mending",
            _ => "Moonlight",
        };

        private GUIStyle? style;

        private void OnGUI()
        {
            var gm = GameManager.instance;
            if (!InRun || !WitchKnightMod.Settings.ShowHud || gm == null || HeroController.instance == null
                || gm.IsNonGameplayScene() || gm.IsGamePaused()) return;

            var scale = Screen.height / 1080f;
            style ??= new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.UpperCenter };
            style.fontSize = Mathf.RoundToInt(22 * scale);

            var room = InBossRoom ? "Champion" : $"Room {RoomIndex + 1}/{RoomsInRegion}";
            var top = $"{Route.Name(Route.Current)} · {RegionName} ({RegionIndex + 1}/{RegionCount}) · {room}";
            var bottom = Cleared
                ? $"Cleared: {RewardName(Reward)}"
                : FoesLeft > 0 ? $"Reward: {RewardName(Reward)}, {FoesLeft} foes left" : $"Reward: {RewardName(Reward)}";

            var w = 900 * scale;
            var x = (Screen.width - w) / 2f;
            var lh = 30 * scale;
            Label(new Rect(x, 16 * scale, w, lh), top, Route.Colour(Route.Current));
            Label(new Rect(x, 16 * scale + lh, w, lh), bottom, Cleared ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 1f, 1f, 0.8f));
        }

        private void Label(Rect r, string s, Color c)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.8f);
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), s, style!);
            GUI.color = c;
            GUI.Label(r, s, style!);
            GUI.color = Color.white;
        }
    }
}
