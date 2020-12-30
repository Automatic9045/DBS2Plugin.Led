using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbsPlugin.Standard.Led
{
    public class LedPartBitmapExtension
    {
        public LedPartBitmap Bitmap { get; internal set; }

        public string ParentControlName { get; internal set; }
        public string ParentPartName { get; internal set; }

        public int DrawWidth { get; internal set; }
        public int DrawHeight { get; internal set; }
    }

    public class LedPartDefinitionExtension
    {
        public LedPartDefinition Definition { get; internal set; }
        public int Y { get; internal set; }
        public LedPartBitmapExtension ParentBitmap { get; internal set; }
    }

    public class LedPartBitmapExtensionEqualityComparer : IEqualityComparer<LedPartBitmapExtension>
    {
        public bool Equals(LedPartBitmapExtension x, LedPartBitmapExtension y)
        {
            return x.Bitmap.Path == y.Bitmap.Path && x.Bitmap.Definitions.ConvertAll(d => d.Name).SequenceEqual(y.Bitmap.Definitions.ConvertAll(d => d.Name));
        }

        public int GetHashCode(LedPartBitmapExtension obj)
        {
            return obj.Bitmap.Path.GetHashCode();
        }
    }
}
