using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.UI
{
    public class EmotePanel : IInitializable
    {
        private FloatingScreen floatingScreen;
        private bool parsed;
        private Vector3 screenPosition;
        private Vector3 screenAngles;

        public void Initialize()
        {
            parsed = false;
        }

        private void Parse()
        {
            if (!parsed)
            {
                parsed = true;
                floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(75, 30), true, new Vector3(0, 0.25f, 1), new Quaternion(0, 0, 0, 0));
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "MultiplayerExtensions.UI.EmotePanel.bsml"), floatingScreen.gameObject, this);
                floatingScreen.gameObject.SetActive(false);
                floatingScreen.gameObject.name = "MultiplayerEmotePanel";
                floatingScreen.transform.localEulerAngles = new Vector3(50, 0);
                screenPosition = floatingScreen.transform.position;
                screenAngles = floatingScreen.transform.localEulerAngles;
            }
            // Restore position so it respawns where we expect it to
            floatingScreen.transform.position = screenPosition;
            floatingScreen.transform.localEulerAngles = screenAngles;
        }

        internal void ToggleActive()
        {
            Parse();
            floatingScreen.gameObject.SetActive(!floatingScreen.gameObject.activeSelf);
        }

        [UIAction("close-screen")]
        private void CloseScreen() => floatingScreen.gameObject.SetActive(false);
    }
}
