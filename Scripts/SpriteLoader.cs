using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class SpriteLoader
{
    #region Private Fields
    private static readonly SpriteMeshType spriteType = SpriteMeshType.Tight;
    private const float PixelsPerUnit = 100.0f;
    #endregion

    public static Sprite GetSpriteFromBytes(byte[] bytes)
    {
        Texture2D photoTexture = LoadTextureByBytes(bytes);
        if (photoTexture == null)
        {
            Debug.LogError("Can't GetSpriteFromFile, cuz photoTexture is null!");
            return null;
        }
        Sprite photoSprite = Sprite.Create(photoTexture,
            new Rect(0, 0, photoTexture.width, photoTexture.height),
            new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
        return photoSprite;
    }

    public static Sprite GetSpriteFromFile(string pathToSprite)
    {
        Texture2D photoTexture = LoadTextureByFile(pathToSprite);
        if (photoTexture == null)
        {
            Debug.LogError("Can't GetSpriteFromFile : "+ pathToSprite);
            return null;
        }
        Sprite photoSprite = Sprite.Create(photoTexture,
            new Rect(0, 0, photoTexture.width, photoTexture.height),
            new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
        return photoSprite;
    }
    public static Sprite GetSpriteFromFileWithCompression(string pathToSprite)
    {
        Texture2D photoTexture = LoadTextureByFile(pathToSprite);
        if (photoTexture == null)
        {
            Debug.LogError("Can't GetSpriteFromFile, cuz photoTexture is null!");
            return null;
        }
       // Texture2D photoTextureCompresed = new Texture2D(photoTexture.width / 2, photoTexture.height / 2, TextureFormat.RGB24, false);
        TextureScale.Bilinear(photoTexture, photoTexture.width / 3, photoTexture.height / 3);
        Sprite photoSprite = Sprite.Create(photoTexture,
            new Rect(0, 0, photoTexture.width, photoTexture.height),
            new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
        return photoSprite; 
        
    }

    public static void SaweSpriteMini(string pathToSprite)
    {
        Texture2D photoTexture = LoadTextureByFile(pathToSprite);
        if (photoTexture == null)
        {
            Debug.LogError("Can't GetSpriteFromFile, cuz photoTexture is null!");
        }
        // Texture2D photoTextureCompresed = new Texture2D(photoTexture.width / 2, photoTexture.height / 2, TextureFormat.RGB24, false);
        TextureScale.Bilinear(photoTexture, photoTexture.width / 5, photoTexture.height / 5);
       
        File.WriteAllBytes(pathToSprite + "mini", photoTexture.EncodeToPNG());
        //Debug.Log("SvedMini : "+ pathToSprite);

    }

    private static Texture2D LoadTextureByFile(string FilePath)
    {
        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);
            if (Tex2D.LoadImage(FileData))
                return Tex2D;
        }
        return null;
    }

    private static Texture2D LoadTextureByBytes(byte[] FileData)
    {
        Texture2D Tex2D = null;

        Tex2D = new Texture2D(2, 2);
        if (FileData.Length > 0)
        {
            Tex2D.LoadImage(FileData);
            //TextureScaler.Bilinear(Tex2D, 256, 256);
        }
        return Tex2D;
    }
}
