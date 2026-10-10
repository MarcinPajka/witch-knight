using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// Saved to WitchKnight.GlobalSettings.json next to the game's saves. Keys are Unity
    /// KeyCode names, so they can be changed by editing that file.
    /// </summary>
    public class GlobalSettings
    {
        public KeyCode MoonHexKey = KeyCode.Q;
        public KeyCode ParryKey = KeyCode.E;
        public KeyCode RedrawKey = KeyCode.R;
        public bool ShowHud = true;
        public bool GreekNames = true;
        public bool RunLoop = true;
        public int RoomsPerRegion = 6;
        public bool BossTwists = true;
    }
}
