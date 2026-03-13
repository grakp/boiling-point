using UnityEngine;

public static class HoverHighlightHelper
{
    static readonly int OutlineThicknessId = Shader.PropertyToID("_OutlineThickness");
    static readonly int OutlineColourId = Shader.PropertyToID("_OutlineColour");
    static readonly Color DefaultHighlightColour = Color.white;
    static MaterialPropertyBlock _block;

    static MaterialPropertyBlock Block => _block ??= new MaterialPropertyBlock();

    public static void ApplyHighlight(SpriteRenderer[] renderers, bool on, Color? highlightColour = null)
    {
        if (renderers == null || renderers.Length == 0) return;

        Color colour = highlightColour ?? DefaultHighlightColour;
        if (on)
        {
            Block.SetFloat(OutlineThicknessId, 1f);
            Block.SetColor(OutlineColourId, colour);
            foreach (var r in renderers)
            {
                if (r != null) r.SetPropertyBlock(Block);
            }
        }
        else
        {
            foreach (var r in renderers)
            {
                if (r != null) r.SetPropertyBlock(null);
            }
        }
    }

    public static void ApplyHighlight(SpriteRenderer renderer, bool on, Color? highlightColour = null)
    {
        if (renderer == null) return;

        if (on)
        {
            Color colour = highlightColour ?? DefaultHighlightColour;
            Block.SetFloat(OutlineThicknessId, 1f);
            Block.SetColor(OutlineColourId, colour);
            renderer.SetPropertyBlock(Block);
        }
        else
        {
            renderer.SetPropertyBlock(null);
        }
    }

    public static void ApplyColourOnly(SpriteRenderer renderer, Color colour)
    {
        if (renderer == null) return;
        renderer.color = colour;
    }
}
