using System;
using UnityEngine;

namespace neeksdk.Scripts.Properties
{
    [RequireComponent(typeof(LineRenderer))]
    public class LnrColorChanger : MonoBehaviour
    {
        [SerializeField] private ColorPreset[] _colorPresets;
        
        private MaterialPropertyBlock _mpb;
        private LineRenderer _lpr;
        
        private static readonly int ShaderPropertyTintColor = Shader.PropertyToID("_TintColor");
        private static readonly int ShaderPropertyTexture = Shader.PropertyToID("_MainTex");

        LineRenderer Lpr
        {
            get
            {
                if (_lpr == null)
                {
                    _lpr = GetComponent<LineRenderer>();
                }

                return _lpr;
            }
        }
        MaterialPropertyBlock Mbp
        {
            get
            {
                if (_mpb == null)
                {
                    _mpb = new MaterialPropertyBlock();
                }

                return _mpb;
            }
        }

        public void ApplyAlpha(ColorTypes colorTypes)
        {
            Mbp.SetColor(ShaderPropertyTintColor, GetColor(colorTypes));
            Lpr.SetPropertyBlock(Mbp);
        }

        public void ApplyTexture(Texture2D texture)
        {
            Mbp.SetTexture(ShaderPropertyTexture, texture);
            Lpr.SetPropertyBlock(Mbp);
        }

        private Color GetColor(ColorTypes colorType)
        {
            foreach (ColorPreset colorPreset in _colorPresets)
            {
                if (colorPreset.ColorType == colorType)
                {
                    return colorPreset.Color;
                }
            }
            
            return Color.white;
        }
        
        [Serializable]
        private class ColorPreset
        {
            public ColorTypes ColorType;
            public Color Color;
        }
    }
}