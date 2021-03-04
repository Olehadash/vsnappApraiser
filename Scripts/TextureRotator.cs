﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureRotator
{
    public static Sprite RotateSprite(Sprite originalSprite)
    { // без контекста не юзать!!! не настроен поворот, нужно переписывать под свои нужды
        Texture2D rotatedTexture = RotateTexture(originalSprite.texture, true);
        Sprite rotatedSprite = Sprite.Create(rotatedTexture,
            new Rect(0, 0, rotatedTexture.width, rotatedTexture.height),
            Vector2.zero);
        return rotatedSprite;
    }

    public static Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }
}
