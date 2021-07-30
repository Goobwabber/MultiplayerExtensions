using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Utilities
{
    internal class ResourceLoader : IDisposable
    {
        private AssetBundle? _bundle;
        private Material? _cachedMaterial;
        private const string RESOURCE_PATH = "MultiplayerExtensions.Assets.sprite.assetbundle";
        private bool _isProcessing = false;

        public async Task<Material> LoadSpriteMaterial()
        {
            while (_isProcessing)
                await SiraUtil.Utilities.AwaitSleep(10);

            if (_cachedMaterial != null)
                return _cachedMaterial;

            _isProcessing = true;
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_PATH);
            using MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            if (_cachedMaterial != null)
                return _cachedMaterial;

            var bundle = AssetBundle.LoadFromMemoryAsync(ms.ToArray());
            while (!bundle.isDone)
                await SiraUtil.Utilities.AwaitSleep(0);
            if (_cachedMaterial != null)
                return _cachedMaterial;

            _bundle = bundle.assetBundle;
            var spriteReq = bundle.assetBundle.LoadAssetAsync<GameObject>("_Sprite");
            while (!spriteReq.isDone)
                await SiraUtil.Utilities.AwaitSleep(0);
            if (_cachedMaterial != null)
                return _cachedMaterial;

            _cachedMaterial = ((GameObject)spriteReq.asset).GetComponent<Renderer>().material;


            _bundle.Unload(false);
            _isProcessing = false;
            return _cachedMaterial;
        }

        public void Dispose()
        {
            if (_bundle != null)
                _bundle.Unload(true);
        }
    }
}