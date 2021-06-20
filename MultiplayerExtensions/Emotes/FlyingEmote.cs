using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerExtensions.Emotes
{
    public class FlyingEmote : Image
    {
        private int MAX_TIME = 3;
        private float time;

        internal void Setup(Sprite sprite, Vector3 position, Quaternion rotation)
        {
            gameObject.SetActive(false);

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3.44f;
            scaler.referencePixelsPerUnit = 10f;

            Canvas canvas = gameObject.GetComponent<Canvas>();
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
            canvas.sortingOrder = 4;

            this.sprite = sprite;
            time = 0;
            rectTransform.sizeDelta = new Vector2(0.4f, 0.4f);
            rectTransform.position = position;
            rectTransform.rotation = rotation;
            gameObject.SetActive(true);
        }

        public void Update()
        {
            transform.Translate(new Vector3(0, 0, 1f) * Time.deltaTime);
            time += Time.deltaTime;
            if (time >= MAX_TIME)
            {
                GameObject.Destroy(this);
            }
        }
    }
}
