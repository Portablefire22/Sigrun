using Microsoft.Extensions.Logging;
using Sigrun.Logging;
using Veldrid;
using Veldrid.ImageSharp;

namespace Sigrun.Engine;

public static class TextureHandler
{
    public static Dictionary<string, uint> TextureIndex { get; private set; } = [];
    private static uint _nextIndex;

    private static Dictionary<string, ImageSharpTexture> _texturesToUpload = [];

    private static bool _newTextures = false;
    public static Dictionary<string, ResourceSet> TextureSets { get; private set; } = [];

    /// <summary>
    /// Submits a given image file to be uploaded to the GPU for rendering.
    /// </summary>
    /// <param name="path">Image name to be uploaded</param>
    /// <returns>Texture's index in texture array</returns>
    public static uint AddTexture(string path)
    {
        var logger = LoggingProvider.NewLogger("Sigrun.texta");
        try
        {
            if (TextureIndex.TryGetValue(path, out var texture)) return texture;
            var tex = new ImageSharpTexture($"Assets/Textures/{path}");
            TextureIndex.Add(path, _nextIndex);
            _texturesToUpload.Add(path, tex);
            _newTextures = true;
            return _nextIndex++;
        }
        catch (Exception e)
        {
            logger.LogError($"{e}");
            return 0;
        }
    }

    public static ResourceSet GetTextureSet(string name)
    {
        return TextureSets.TryGetValue(name, out var set) ? set : TextureSets["missingTexture.jpg"];
    }

    public static void CreateSets(GraphicsDevice graphicsDevice)
    {
        if (!_newTextures) return;
        var factory = graphicsDevice.ResourceFactory;
        foreach (var (name, tex) in _texturesToUpload)
        {
            var textureView = factory.CreateTextureView(tex.CreateDeviceTexture(graphicsDevice, factory));
            
            var worldTextureLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SurfaceTextures", ResourceKind.TextureReadOnly,
                        ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            var textureSet =
                factory.CreateResourceSet(new ResourceSetDescription(worldTextureLayout, textureView,
                    graphicsDevice.Aniso4xSampler));
            TextureSets.Add(name, textureSet);   
        }
        _texturesToUpload.Clear();
        _newTextures = false;
    }

    public static void Dispose()
    {
        foreach (var (_, set) in TextureSets)
        {
            set.Dispose();
        }
    }
}