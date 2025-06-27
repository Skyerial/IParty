using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;

/**
 * @brief Generates a QR code from a given text and displays it on a RawImage component.
 * Used in the lobby and mobile testing scene.
 */
public class QRCodeGenerator : MonoBehaviour
{
    /**
     * @brief Generates a QR code from the specified text and assigns it to the target RawImage.
     *
     * @param text The text to encode in the QR code.
     * @param targetRenderer The RawImage component where the QR code will be displayed.
     */
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
        // targetRenderer.material.mainTexture = qrTexture;
        targetRenderer.texture = qrTexture;
    }
}
