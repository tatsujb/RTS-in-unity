using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureNoise : MonoBehaviour {

    
    public static Texture2D MakeTexture(int height, int width, int octaves, float noiseScale) {
        Texture2D noiseTex = new Texture2D(width, height);
        noiseTex.filterMode = FilterMode.Point;

        for(int x = 0; x < noiseTex.width; x++) {
            for(int y = 0; y < noiseTex.height; y++) {
                float noiseValue = 0;
                
                NoiseS3D.octaves = octaves;
                noiseValue = (float)NoiseS3D.NoiseCombinedOctaves(x * noiseScale, y * noiseScale);             
                    
                

                //remap the value to 0 - 1 for color purposes
                noiseValue = (noiseValue + 1) * 0.5f;

                noiseTex.SetPixel(x, y, new Color(noiseValue, noiseValue, noiseValue));
            }
        }

        return noiseTex;

    }
}
