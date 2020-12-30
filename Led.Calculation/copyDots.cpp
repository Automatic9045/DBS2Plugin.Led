#include "stdafx.h"
#include "core.h"

namespace led_drawing_calculator
{
	void copyDots(BYTE destination[], BYTE source[], int width, int height, int destX, int destY, int destWidth, int destHeight, int sourceX, int sourceY, int sourceWidth, int sourceHeight, bool enableTransparent)
	{
		for (int currentY = 0; currentY < height; currentY++)
		{
			if (destY + currentY < 0 || sourceY + currentY < 0) continue;
			if (destHeight <= destY + currentY || sourceHeight <= sourceY + currentY) break;

			for (int currentX = 0; currentX < width; currentX++)
			{
				if (destX + currentX < 0 || sourceX + currentX < 0) continue;
				if (destWidth <= destX + currentX || sourceWidth <= sourceX + currentX) break;

				int pos_dest = (destX + currentX + (destY + currentY) * destWidth) * 3;
				int pos_source = (sourceX + currentX + (sourceY + currentY) * sourceWidth) * 3;

				if (!enableTransparent || source[pos_source] > 0x33 || source[pos_source + 1] > 0x33 || source[pos_source + 2] > 0x33)
				{
					destination[pos_dest] = source[pos_source];
					destination[pos_dest + 1] = source[pos_source + 1];
					destination[pos_dest + 2] = source[pos_source + 2];
				}
			}
		}
	}
}