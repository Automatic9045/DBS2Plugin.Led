using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace DbsPlugin.Standard.Led
{
    internal class LedControl
    {
        internal string ControlName { get; set; }

        internal int Width { get; set; }
        internal int Height { get; set; }
        internal int Stride { get; set; }
        internal int DotWidth { get; set; }
        internal int DotHeight { get; set; }
        internal int DotStride { get; set; }
        internal int DotDiameter { get; set; }
        internal double DotRadius { get; set; }
        internal double DotXDistance { get; set; }
        internal double DotYDistance { get; set; }

        internal WriteableBitmap Bitmap { get; set; }
        internal byte[] DotPixels { get; set; }
        internal byte[] Pixels { get; set; }

        internal Func<List<LedPartBitmapExtension>> GetPartBitmapsForRichTextFunc { get; set; }

        internal List<LedPart> Parts { get; set; }

        internal List<LedShortcut> Shortcuts { get; set; }
        internal List<LedFont> Fonts { get; set; }

        internal LedFont DefaultFont { get; set; }
        internal LedFont BiggestFont { get; set; }

        internal RichText FullText { get; set; }
        internal string FullTextContent { get; set; } = "";
        internal bool UseFullText { get; set; } = false;
        internal bool ScrollFullText { get; set; } = false;
        internal long FunnTextScrollStartedTime { get; set; } = 0;

        internal LedDisplayMode Mode { get; set; }

        internal void SetFullText()
        {
            FullText = new RichText($"{{hdef:{BiggestFont.SystemName}}}{{font:{DefaultFont.SystemName}}}" + FullTextContent, Fonts, GetPartBitmapsForRichTextFunc.Invoke(), -1, DotHeight);
        }

        internal void SetBlankFullText()
        {
            FullText = new RichText(DotWidth, DotHeight);
        }


        /// <summary>
        /// [px/s] スクロールの速度。
        /// </summary>
        internal static readonly int ScrollSpeed = 36;

        /// <summary>
        /// [ms] スクロールし切ってから再度右側に表示されるまでの時間。
        /// </summary>
        internal static readonly int TimeToRescroll = 6000;

        /// <summary>
        /// [ms] stop (stp) コマンドで一時停止する時間。
        /// </summary>
        internal static readonly int StoppageTime = 3000;

        internal int GetScrollingFullTextX(int milliseconds)
        {
            int span = (FullText.Width + DotWidth) * 1000 / ScrollSpeed + StoppageTime * FullText.StopPositions.Count + TimeToRescroll;
            milliseconds %= span;

            for (int i = 0; i < FullText.StopPositions.Count; i++)
            {
                int timeToStopPosition = (DotWidth + FullText.StopPositions[i]) * 1000 / ScrollSpeed + StoppageTime * i;

                if (milliseconds < timeToStopPosition) // 経過時間＜停止位置までの所要時間のとき ⇔ 停止位置手前でスクロール中のとき
                {
                    return DotWidth - ScrollSpeed * (milliseconds - StoppageTime * i) / 1000;
                }
                else if (timeToStopPosition <= milliseconds && milliseconds <= timeToStopPosition + StoppageTime) // 停止位置までの所要時間≦経過時間≦停止位置までの所要時間＋停止時間のとき ⇔ 停止中のとき
                {
                    return -FullText.StopPositions[i];
                }
            }

            return DotWidth - ScrollSpeed * (milliseconds - StoppageTime * FullText.StopPositions.Count) / 1000;
        }
    }

    enum LedDisplayMode
    {
        Normal = 0,
        Fps,
        ElapsedMilliseconds,
        Debug,
    }
}
