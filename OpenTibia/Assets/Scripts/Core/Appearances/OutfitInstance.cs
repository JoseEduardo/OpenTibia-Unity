﻿using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public class OutfitInstance : AppearanceInstance
    {
        public const int InvisibleOutfitId = 0;

        protected int _headHsiColor = 0;
        protected int _torsoHsiColor = 0;
        protected int _legsHsiColor = 0;
        protected int _detailHsiColor = 0;
        protected int _addOns = 0;

        private Color _headColor = Color.white;
        private Color _torsoColor = Color.white;
        private Color _legsColor = Color.white;
        private Color _detailColor = Color.white;

        private int _phase = 0;
        private bool _walking = false;

        public int Head { get => _headHsiColor; }
        public int Torso { get => _torsoHsiColor; }
        public int Legs { get => _legsHsiColor; }
        public int Detail { get => _detailHsiColor; }
        public int AddOns { get => _addOns; }

        public OutfitInstance(uint id, AppearanceType type, int head, int torso, int legs, int feet, int addons) : base(id, type) {
            UpdateProperties(head, torso, legs, feet, addons);
        }

        public void UpdateProperties(int head, int torso, int legs, int detail, int addons) {
            if (_headHsiColor != head) {
                _headHsiColor = head;
                _headColor = Colors.ColorFromHSI(head);
            }

            if (_torsoHsiColor != torso) {
                _torsoHsiColor = torso;
                _torsoColor = Colors.ColorFromHSI(torso);
            }

            if (_legsHsiColor != legs) {
                _legsHsiColor = legs;
                _legsColor = Colors.ColorFromHSI(legs);
            }

            if (_detailHsiColor != detail) {
                _detailHsiColor = detail;
                _detailColor = Colors.ColorFromHSI(detail);
            }

            if (_addOns != addons)
                _addOns = addons;
        }

        public override int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            return (int)((((
                (layer >= 0 ? layer : (_phase > 0 ? _phase : 0)) % _activeFrameGroup.SpriteInfo.Phases
                * _activeFrameGroup.SpriteInfo.PatternDepth + (patternZ >= 0 ? (int)(patternZ % _activeFrameGroup.SpriteInfo.PatternDepth) : 0))
                * _activeFrameGroup.SpriteInfo.PatternHeight + (patternY >= 0 ? (int)(patternY % _activeFrameGroup.SpriteInfo.PatternHeight) : 0))
                * _activeFrameGroup.SpriteInfo.PatternWidth + (patternX >= 0 ? (int)(patternX % _activeFrameGroup.SpriteInfo.PatternWidth) : 0))
                * _activeFrameGroup.SpriteInfo.Layers);
        }

        public override void Draw(Vector2 screenPosition, Vector2 zoom, int patternX, int patternY, int patternZ, bool highlighted = false, float highlightOpacity = 0) {
            if (_activeFrameGroup.SpriteInfo.Layers != 2) {
                var cachedSprite = GetSprite(-1, patternX, patternY, patternZ, _activeFrameGroup.SpriteInfo.IsAnimation);
                if (cachedSprite != null)
                    InternalDrawTo(screenPosition.x, screenPosition.y, zoom, highlighted, highlightOpacity, cachedSprite);
                return;
            }

            var colouriseMaterial = OpenTibiaUnity.GameManager.OutfitTypeMaterial;
            colouriseMaterial.SetColor("_HeadColor", _headColor);
            colouriseMaterial.SetColor("_TorsoColor", _torsoColor);
            colouriseMaterial.SetColor("_LegsColor", _legsColor);
            colouriseMaterial.SetColor("_DetailColor", _detailColor);

            bool dontDraw = false;
            for (patternY = 0; patternY < _activeFrameGroup.SpriteInfo.PatternHeight; patternY++) {
                if (patternY > 0 && (_addOns & 1 << (patternY - 1)) == 0)
                    continue;

                int spriteIndex = GetSpriteIndex(-1, patternX, patternY, patternZ);
                uint spriteId = _activeFrameGroup.SpriteInfo.SpriteIDs[spriteIndex];

                CachedSprite baseSprite, colorSprite;
                OpenTibiaUnity.AppearanceStorage.GetSprite(_activeFrameGroup.SpriteInfo.SpriteIDs[spriteIndex++], out baseSprite);
                OpenTibiaUnity.AppearanceStorage.GetSprite(_activeFrameGroup.SpriteInfo.SpriteIDs[spriteIndex], out colorSprite);

                // if these are not loaded yet we should still continue to
                // ensure that next time all the needed layers are loaded!
                if (baseSprite == null || colorSprite == null)
                    dontDraw = true;

                if (!dontDraw) {
                    colouriseMaterial.SetTexture("_ChannelsTex", colorSprite.texture);
                    colouriseMaterial.SetTextureOffset("_ChannelsTex", colorSprite.rect.position - baseSprite.rect.position);

                    InternalDrawTo(screenPosition.x, screenPosition.y, zoom, highlighted, highlightOpacity, baseSprite);
                    InternalDrawTo(screenPosition.x, screenPosition.y, zoom, highlighted, highlightOpacity, baseSprite, colouriseMaterial);
                }
            }
        }

        public override void SwitchFrameGroup(int ticks, int frameGroupIndex) {
            bool updateAnimator = false, forceUpdate = false;
            bool walking = _walking;

            _walking = frameGroupIndex == (int)Protobuf.Shared.FrameGroupType.Walking;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameSeparateAnimationGroups)) {
                frameGroupIndex = Mathf.Min(frameGroupIndex, _type.FrameGroups.Count - 1);
                if (frameGroupIndex != _activeFrameGroupIndex) {
                    _activeFrameGroupIndex = frameGroupIndex;
                    updateAnimator = true;
                    forceUpdate = !_walking;
                }
            } else if (_walking != walking) {
                updateAnimator = forceUpdate = true;
            }

            if (updateAnimator) {
                var animator = _animators?[_activeFrameGroupIndex];
                if (animator != null && (animator.LastAnimationTick + Constants.AnimationDelayBeforeReset < ticks || forceUpdate))
                    animator.Reset();
            }
        }

        public override bool Animate(int ticks, int delay = 0) {
            Animation.IAppearanceAnimator animator = _animators?[_activeFrameGroupIndex];
            if (animator != null) {
                animator.Animate(ticks, delay);
                _phase = animator.Phase;
                return !animator.Finished;
            }
            return false;
        }
    }
}
