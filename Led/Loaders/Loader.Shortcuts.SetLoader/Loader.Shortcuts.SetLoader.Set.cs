using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;

using static DbsPlugin.Standard.Led.Led;

namespace DbsPlugin.Standard.Led
{
	internal partial class Loader
    {
        private partial class ShortcutSetLoader
        {
            internal LedShortcutSet LoadSet(XElement setElement)
            {
                LedShortcutSet set = new LedShortcutSet(parts);

                int? targetIndex = (int?)setElement.Attribute("TargetIndex");
                string targetName = (string)setElement.Attribute("Target");
                if (targetIndex != null)
                {
                    if (0 <= targetIndex && targetIndex < parts.Count)
                        set.TargetIndex = (int)targetIndex;
                    else
                    {
                        throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないパーツ ID \"" + targetIndex + "\" が指定されています。");
                        return null;
                    }
                }
                else if (targetName != null)
                {
                    if (parts.Any(p => p.SystemName == targetName))
                        set.TargetSystemName = targetName;
                    else
                    {
                        throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないパーツ システム名 \"" + targetName + "\" が指定されています。");
                        return null;
                    }
                }
                else
                {
                    throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、変更先のパーツが指定されていない Set があります。");
                    return null;
                }

                int? imageIndex = (int?)setElement.Attribute("ImageIndex");
                string imageName = (string)setElement.Attribute("Image");
                if (targetIndex != null)
                {
                    if (0 <= imageIndex && imageIndex < parts[set.TargetIndex].Bitmaps.Count)
                        set.ImageIndex = (int)imageIndex;
                    else
                    {
                        throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないグループ ID \"" + imageIndex + "\" （パーツ：\"" + set.TargetSystemName + "\" ）が指定されています。");
                        return null;
                    }
                }
                else if (imageName != null)
                {
                    if (parts[set.TargetIndex].Bitmaps.Any(p => p.SystemName == imageName))
                        set.ImageSystemName = imageName;
                    else
                    {
                        throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないグループ システム名 \"" + imageName + "\" （パーツ：\"" + set.TargetSystemName + "\" ）が指定されています。");
                        return null;
                    }
                }
                else if (parts[set.TargetIndex].Bitmaps.Count == 1)
                {
                    set.ImageIndex = 0;
                }
                else
                {
                    throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、変更先のグループが指定されていない Set があります。変更先のグループの指定が不要なのは、そのパーツにグループが１つしか存在しないときのみです。");
                    return null;
                }

                int? frameIndex = (int?)setElement.Attribute("FrameIndex");
                string frameName = (string)setElement.Attribute("Frame");
                if (frameIndex != null)
                {
                    if (-1 <= frameIndex && frameIndex < parts[set.TargetIndex].Bitmaps[set.ImageIndex].Definitions.Count)
                        set.FrameIndex = (int)frameIndex;
                    else
                    {
                        throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないコマ ID \"" + frameIndex + "\" （パーツ：\"" + set.TargetSystemName + "\"、グループ：\"" + set.ImageSystemName + "\" ）が指定されています。");
                        return null;
                    }
                }
                else if (frameName == ";Null;")
                {
                    set.FrameIndex = -1;
                }
                else if (frameName != null)
                {
                    if (parts[set.TargetIndex].Bitmaps[set.ImageIndex].Definitions.Any(p => p.SystemName == frameName))
                        set.FrameSystemName = frameName;
                    else
                    {
                        throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないコマ システム名 \"" + frameName + "\" （パーツ：\"" + set.TargetSystemName + "\"、グループ：\"" + set.ImageSystemName + "\" ）が指定されています。");
                        return null;
                    }
                }
                else
                {
                    throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、変更先のコマが指定されていない Set があります。");
                    return null;
                }

                return set;
            }
        }
    }
}
