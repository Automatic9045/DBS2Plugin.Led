#include "stdafx.h"
#include "core.h"

namespace led_drawing_calculator
{
	void clearDots(BYTE dots[], int width, int height)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				dots[(x + y * width) * 3] = 0x33;
				dots[(x + y * width) * 3 + 1] = 0x33;
				dots[(x + y * width) * 3 + 2] = 0x33;
			}
		}
	}
}