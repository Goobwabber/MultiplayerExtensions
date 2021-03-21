using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MPGameInstaller : MonoInstaller
    {
        public override void InstallBindings() { }

        public override void Start()
        {
            List<SimpleColorSO> colors = Resources.FindObjectsOfTypeAll<SimpleColorSO>().ToList();
            colors.Find(color => color.name == "MultiplayerFailedPlayerColor").SetColor(new Color(1f, 0f, 0f, 0.5f));
        }
    }
}
