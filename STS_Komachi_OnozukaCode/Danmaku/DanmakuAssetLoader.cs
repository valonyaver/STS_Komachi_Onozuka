using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using System;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku.Nodes;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku
{

    public static class DanmakuAssetLoader
    {
        public static Texture2D LoadBulletSprite(string path)
        {
            Resource res = ResourceLoader.Load(path);
            if (res is Texture2D tex) return tex;

            throw new InvalidOperationException(
                $"[Danmaku] Expected a sprite (Texture2D) for a bullet at '{path}', but loaded a " +
                $"{res?.GetType().Name ?? "null resource"}. If this piece is meant to be a laser, " +
                $"set IsLaser = true on the DanmakuPiece.");
        }

        public static NDanmakuLaser InstantiateLaser(string path)
        {
            Resource res = ResourceLoader.Load(path);
            if (res is not PackedScene scene)
            {
                throw new InvalidOperationException(
                    $"[Danmaku] Expected a laser scene (PackedScene) at '{path}', but loaded a " +
                    $"{res?.GetType().Name ?? "null resource"}. If this piece is meant to be a bullet, " +
                    $"set IsLaser = false on the DanmakuPiece.");
            }

            Node instance = scene.Instantiate();
            if (instance is not NDanmakuLaser laser)
            {
                instance.QueueFree();
                throw new InvalidOperationException(
                    $"[Danmaku] Scene at '{path}' has a root node of type {instance.GetType().Name}, " +
                    $"which doesn't derive from NDanmakuLaser. Laser scenes must use NDanmakuLaser " +
                    $"or a subclass as their root script.");
            }

            return laser;
        }
    }
}
