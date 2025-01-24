using NonsensicalKit.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.UGUI
{
    public delegate IEnumerator GetSpriteHandle(ref Sprite sp);

    /// <summary>
    /// 精灵管理类
    /// </summary>
    public class SpriteManager : MonoSingleton<SpriteManager>
    {
        [SerializeField] private bool m_autoDestroy;
        private readonly Dictionary<string, SpriteInfo> _crtSprites = new();

        private void Update()
        {
            UnloadUnused();
            enabled = false;
        }

        public void TrySetSprite(string spriteName, GetSpriteHandle spriteCreateMethod)
        {
            if (_crtSprites.TryGetValue(spriteName, out var sprite))
            {
                if (sprite.CanUse)
                {
                    return;
                }
            }
            _crtSprites[spriteName] = new SpriteInfo(spriteCreateMethod);
        }

        public void SetSprite(string spriteName, GetSpriteHandle spriteCreateMethod)
        {
            _crtSprites[spriteName] = new SpriteInfo(spriteCreateMethod);
        }

        public void TrySetSprite(string spriteName, Func<Sprite> spriteCreateMethod)
        {
            if (_crtSprites.TryGetValue(spriteName, out var sprite))
            {
                if (sprite.CanUse)
                {
                    return;
                }
            }
            _crtSprites[spriteName] = new SpriteInfo(spriteCreateMethod);
        }

        public void SetSprite(string spriteName, Func<Sprite> spriteCreateMethod)
        {
            _crtSprites[spriteName] = new SpriteInfo(spriteCreateMethod);
        }

        public void TryGetSprite(string spriteName, Action<Sprite> callback, GetSpriteHandle fallback = null)
        {
            StartCoroutine(TryGetSpriteCoroutine(spriteName, callback, fallback));
        }

        public IEnumerator TryGetSpriteCoroutine(string spriteName, Action<Sprite> callback, GetSpriteHandle fallback)
        {
            Sprite sprite = null;
            if (_crtSprites.ContainsKey(spriteName))
            {
                if (_crtSprites[spriteName].Sprite == null)
                {
                    if (_crtSprites[spriteName].UseFunc)
                    {
                        sprite = _crtSprites[spriteName].SpriteCreateFunc();
                    }
                    else
                    {
                        yield return _crtSprites[spriteName].SpriteCreateMethod(ref sprite);
                    }
                    if (sprite != null)
                    {
                        _crtSprites[spriteName].Sprite = sprite;
                    }
                }
                sprite = _crtSprites[spriteName].Sprite;
                if (sprite != null)
                {
                    _crtSprites[spriteName].UseCount++;
                }
            }

            if (sprite == null && fallback != null)
            {
                yield return fallback(ref sprite);
                if (sprite != null)
                {
                    _crtSprites[spriteName] = new SpriteInfo
                    {
                        Sprite = sprite
                    };

                    _crtSprites[spriteName].UseCount++;
                }
            }

            callback?.Invoke(sprite);
        }

        public IEnumerator TryGetSprite(Sprite sprite, string spriteName, GetSpriteHandle fallback = null)
        {
            if (_crtSprites.ContainsKey(spriteName))
            {
                if (_crtSprites[spriteName].Sprite == null)
                {
                    if (_crtSprites[spriteName].UseFunc)
                    {
                        sprite = _crtSprites[spriteName].SpriteCreateFunc();
                    }
                    else
                    {
                        yield return _crtSprites[spriteName].SpriteCreateMethod(ref sprite);
                    }
                    if (sprite != null)
                    {
                        _crtSprites[spriteName].Sprite = sprite;
                    }
                }
                sprite = _crtSprites[spriteName].Sprite;
                if (sprite != null)
                {
                    _crtSprites[spriteName].UseCount++;
                }
            }

            if (sprite == null && fallback != null)
            {
                yield return fallback(ref sprite);
                if (sprite != null)
                {
                    _crtSprites[spriteName] = new SpriteInfo
                    {
                        Sprite = sprite
                    };

                    _crtSprites[spriteName].UseCount++;
                }
            }
        }

        public void RecoverySprite(string spriteName)
        {
            if (NonsensicalInstance.ApplicationIsQuitting)
            {
                return;
            }
            if (_crtSprites.TryGetValue(spriteName, out var sprite))
            {
                sprite.UseCount--;
                if (m_autoDestroy)
                {
                    enabled = true;
                }
            }
        }

        public void UnloadUnused()
        {
            List<string> unloads = new List<string>();
            foreach (var item in _crtSprites)
            {
                if (item.Value.UseCount == 0)
                {
                    Destroy(item.Value.Sprite);
                    item.Value.Sprite = null;
                    unloads.Add(item.Key);
                }
            }
            foreach (var item in unloads)
            {
                _crtSprites.Remove(item);
            }
        }

        public void Clear()
        {
            foreach (var item in _crtSprites.Values)
            {
                Destroy(item.Sprite);
                item.Sprite = null;
            }
            _crtSprites.Clear();
        }

        private class SpriteInfo
        {
            public Sprite Sprite;
            public GetSpriteHandle SpriteCreateMethod;
            public Func<Sprite> SpriteCreateFunc;
            public int UseCount;
            public bool UseFunc;

            public bool CanUse
            {
                get
                {
                    if (UseFunc)
                    {
                        return SpriteCreateFunc != null;
                    }
                    else
                    {
                        return SpriteCreateMethod != null;
                    }
                }
            }
            public SpriteInfo()
            {
            }

            public SpriteInfo(GetSpriteHandle spriteCreateMethod)
            {
                SpriteCreateMethod = spriteCreateMethod;
                UseFunc = false;
            }
            public SpriteInfo(Func<Sprite> spriteCreateFunc)
            {
                SpriteCreateFunc = spriteCreateFunc;
                UseFunc = true;
            }
        }
    }
}
