#include "stdafx.h"
#include "core.h"

namespace led_drawing_calculator
{
	void fillDots(BYTE dots[], COLOR64 color, int x, int y, int drawWidth, int drawHeight, int width, int height)
	{
		BYTE r = (color >> 16) & 0xff;
		BYTE g = (color >> 8) & 0xff;
		BYTE b = color & 0xff;

		for (int currentY = 0; currentY < drawHeight; currentY++)
		{
			if (y + currentY < 0) continue;
			if (height <= y + currentY) break;

			for (int currentX = 0; currentX < drawWidth; currentX++)
			{
				if (x + currentX < 0) continue;
				if (width <= x + currentX) break;

				dots[(x + currentX + (y + currentY) * width) * 3] = r;
				dots[(x + currentX + (y + currentY) * width) * 3 + 1] = g;
				dots[(x + currentX + (y + currentY) * width) * 3 + 2] = b;
			}
		}
	}
}