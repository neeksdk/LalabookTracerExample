using System;
using UnityEngine;

namespace neeksdk.Scripts.Properties
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SprColorChanger : MonoBehaviour
    {
        [SerializeField] private ColorPreset[] _colorPresets;
        
        private MaterialPropertyBlock _mpb;
        private SpriteRenderer _spr;

        private static readonly int ShaderPropertyColor = Shader.PropertyToID("_Color");

        SpriteRenderer Spr
        {
            get
            {
                if (_spr == null)
                {
                    _spr = GetComponent<SpriteRenderer>();
                }

                return _spr;
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

        public void ApplyColor(ColorTypes colorType)
        {
            Mbp.SetColor(ShaderPropertyColor, GetColor(colorType));
            Spr.SetPropertyBlock(Mbp);
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
