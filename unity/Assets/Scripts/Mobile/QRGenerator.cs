using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;

public class QRCodeGenerator : MonoBehaviour
{
    public static void GenerateQRCode(string text, RawImage targetRenderer)
    {
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Height = 256,
                Width = 256,
                Margin = 1
            }
        };

        var pixelData = writer.Write(text);

        Texture2D texture = new Texture2D(pixelData.Width, pixelData.Height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(pixelData.Pixels);
        texture.Apply();

        Texture2D qrTexture = texture;
        // Optional section specifying the canvas in code.
        // GameObject qrObject = GameObject.Find("SampleQR");
        // RawImage targetRenderer = qrObject.GetComponent<RawImage>();
        targetRenderer.material.mainTexture = qrTexture;
    }
}
