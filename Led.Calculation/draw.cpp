#include "stdafx.h"
#include "core.h"

namespace led_drawing_calculator
{
	void draw(BYTE bmp[], BYTE dots[], int dotWidth, int dotHeight, double dotXDistance, double dotYDistance, double dotRadius, int stride, bool redrawAll)
	{
		int cx, cy, current_r, current_g, current_b, d;
		bool is_not_matches_r, is_not_matches_g, is_not_matches_b;

		int x, y;
		double xx, yy;
		int cxpx, cxmx, cypy, cymy, cxpy, cxmy, cypx, cymx;

		for (int i = 0; i < dotWidth; i++)
		{
			for (int p = 0; p < dotHeight; p++)
			{
				cx = dotXDistance * (i + 1) + dotRadius * (i * 2 + 1);
				cy = dotYDistance * (p + 1) + dotRadius * (p * 2 + 1);
				current_r = dots[(i + dotWidth * p) * 3 + 2];
				current_g = dots[(i + dotWidth * p) * 3 + 1];
				current_b = dots[(i + dotWidth * p) * 3];
				is_not_matches_r = current_r != bmp[(cx << 2) + stride * cy + 2];
				is_not_matches_g = current_g != bmp[(cx << 2) + stride * cy + 1];
				is_not_matches_b = current_b != bmp[(cx << 2) + stride * cy];

				if (redrawAll || is_not_matches_r || is_not_matches_g || is_not_matches_b)
				{
					for (d = dotRadius * 2; d > 0; d -= 2)
					{
						x = 0;
						y = 0;
						xx = d << 6;
						yy = 0;
						while (yy <= xx)
						{
							x = xx / 128;
							y = yy / 128;
							cxpx = cx + x;
							cxmx = cx - x;
							cypy = cy + y;
							cymy = cy - y;
							cxpy = cx + y;
							cxmy = cx - y;
							cypx = cy + x;
							cymx = cy - x;
							if (redrawAll || is_not_matches_b)
							{
								bmp[(cxpx << 2) + stride * cypy] = current_b;
								bmp[(cxmx << 2) + stride * cymy] = current_b;
								bmp[(cxmx << 2) + stride * cypy] = current_b;
								bmp[(cxpx << 2) + stride * cymy] = current_b;
								bmp[(cxpy << 2) + stride * cypx] = current_b;
								bmp[(cxmy << 2) + stride * cymx] = current_b;
								bmp[(cxmy << 2) + stride * cypx] = current_b;
								bmp[(cxpy << 2) + stride * cymx] = current_b;
							}
							if (redrawAll || is_not_matches_g)
							{
								bmp[(cxpx << 2) + stride * cypy + 1] = current_g;
								bmp[(cxmx << 2) + stride * cymy + 1] = current_g;
								bmp[(cxmx << 2) + stride * cypy + 1] = current_g;
								bmp[(cxpx << 2) + stride * cymy + 1] = current_g;
								bmp[(cxpy << 2) + stride * cypx + 1] = current_g;
								bmp[(cxmy << 2) + stride * cymx + 1] = current_g;
								bmp[(cxmy << 2) + stride * cypx + 1] = current_g;
								bmp[(cxpy << 2) + stride * cymx + 1] = current_g;
							}
							if (redrawAll || is_not_matches_r)
							{
								bmp[(cxpx << 2) + stride * cypy + 2] = current_r;
								bmp[(cxmx << 2) + stride * cymy + 2] = current_r;
								bmp[(cxmx << 2) + stride * cypy + 2] = current_r;
								bmp[(cxpx << 2) + stride * cymy + 2] = current_r;
								bmp[(cxpy << 2) + stride * cypx + 2] = current_r;
								bmp[(cxmy << 2) + stride * cymx + 2] = current_r;
								bmp[(cxmy << 2) + stride * cypx + 2] = current_r;
								bmp[(cxpy << 2) + stride * cymx + 2] = current_r;
							}
							yy += xx / 128;
							xx -= yy / 128;
						}
					}
				}
			}
		}
	}
}