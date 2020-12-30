#include "stdafx.h"
#include "core.h"

namespace led_drawing_calculator
{
	void writeImageToDots(BYTE dots[], BYTE source[], int x, int y, int width, int height, int drawWidth, int drawHeight, int dotWidth, int dotHeight, int xIndex, int yIndex, bool enableTransparent)
	{
		int currentX, currentY;
		int position_dots, position_source;

		if (x < 0) currentX = 0; else currentX = x;
		for (; currentX < x + drawWidth && currentX < dotWidth; currentX++)
		{
			if (y < 0) currentY = 0; else currentY = y;
			for (; currentY < y + drawHeight && currentY < dotHeight; currentY++)
			{
				position_dots = (currentX + currentY * dotWidth) * 3;

				if (xIndex == -1 || yIndex == -1)
				{
				}
				else
				{
					if (enableTransparent)
					{
						if (dots[position_dots] != 0x33) dots[position_dots] = 0x33;
						if (dots[position_dots + 1] != 0x33) dots[position_dots + 1] = 0x33;
						if (dots[position_dots + 2] != 0x33) dots[position_dots + 2] = 0x33;
					}

					int sourceX = currentX - x + drawWidth * xIndex;
					int sourceY = currentY - y + drawHeight * yIndex;
					position_source = (sourceX + sourceY * width) << 2;
					if (source[position_source + 3] == 0xff)
					{
						dots[position_dots] = source[position_source];
						dots[position_dots + 1] = source[position_source + 1];
						dots[position_dots + 2] = source[position_source + 2];
					}
				}
			}
		}
	}
}