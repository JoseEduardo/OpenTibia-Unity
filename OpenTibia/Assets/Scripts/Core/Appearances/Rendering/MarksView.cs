﻿using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Rendering
{
    public class MarksView
    {
        private static Color[] s_FrameColors;

        private const int FrameSizesCount = 15;
        private const int CacheDimention = 10;

        static MarksView() {
            s_FrameColors = new Color[Marks.MarksNumTotal];
            for (int i = 0; i < s_FrameColors.Length; i++)
                s_FrameColors[i] = Colors.ColorFrom8Bit(i);

            s_FrameColors[Marks.MarkAim] = Colors.ColorFromRGB(0xFFFFFF);
            s_FrameColors[Marks.MarkAimAttack] = Colors.ColorFromRGB(0xFF8888);
            s_FrameColors[Marks.MarkAimFollow] = Colors.ColorFromRGB(0x88FF88);
            s_FrameColors[Marks.MarkAttack] = Colors.ColorFromRGB(0xFF0000);
            s_FrameColors[Marks.MarkFollow] = Colors.ColorFromRGB(0x00FF00);
        }

        private List<MarksViewInformation> _marksViewInformations;
        private uint _marksStartSize;

        public uint MarksStartSize { get => _marksStartSize; set => _marksStartSize = value; }

        public MarksView(uint marksStartSize = 0) {
            if (marksStartSize >= FrameSizesCount)
                throw new System.Exception("MarksView.MarksView: Invalid marks start size.");
            _marksStartSize = marksStartSize;
            _marksViewInformations = new List<MarksViewInformation>();
        }

        public void AddMarkToView(MarkType markType, uint thinkness) {
            if (thinkness != Constants.MarkThicknessThin && thinkness != Constants.MarkThicknessBold) {
                throw new System.Exception("MarksView.addMarkToView: Invalid marks thickness: " + thinkness);
            }

            uint size = _marksStartSize;
            foreach (var markInformation in _marksViewInformations)
                size = size + markInformation.MarkThickness;

            if (size + thinkness >= FrameSizesCount)
                throw new System.Exception("MarksView.AddMarkToView: Adding this mark will exceed the maximum frame size");

            MarksViewInformation information = new MarksViewInformation();
            information.MarkType = markType;
            information.MarkThickness = thinkness;

            _marksViewInformations.Add(information);
        }

        public void DrawMarks(Marks marks, float screenX, float screenY, Vector2 zoom) {
            Rect screenRect = new Rect() {
                x = screenX * zoom.x,
                y = screenY * zoom.y,
                width = Constants.FieldSize * zoom.x,
                height = Constants.FieldSize * zoom.y,
            };

            var size = _marksStartSize;
            var tex2d = OpenTibiaUnity.GameManager.MarksViewTexture;
            var material = OpenTibiaUnity.GameManager.MarksViewMaterial;

            foreach (var information in _marksViewInformations) {
                if (!marks.IsMarkSet(information.MarkType))
                    continue;

                uint eightBit = marks.GetMarkColor(information.MarkType);
                Color color;
                if (eightBit > Marks.MarksNumTotal)
                    continue;
                else if (eightBit > Marks.MarkNumColors)
                    color = s_FrameColors[(int)eightBit];
                else
                    color = Colors.ColorFrom8Bit((int)eightBit);

                Rect texRect = new Rect() {
                    x = size * Constants.FieldSize / (float)tex2d.width,
                    y = (Constants.MarkThicknessBold - information.MarkThickness) * Constants.FieldSize / (float)tex2d.height,
                    width = Constants.FieldSize / (float)tex2d.width,
                    height = Constants.FieldSize / (float)tex2d.height,
                };

                material.SetColor("_Color", color);
                Graphics.DrawTexture(screenRect, tex2d, texRect, 0, 0, 0, 0, color, material);
                size += information.MarkThickness;
            }
        }
    }

    public class MarksViewInformation
    {
        public MarkType MarkType { get; set; } = MarkType.None;
        public uint MarkThickness { get; set; } = 1;
    }
}
