#include "stdafx.h"
#include "core.h"

using namespace std;

namespace led_drawing_calculator
{
	HDC hdc = GetDC(NULL);
	const MAT2 mat = { {0, 1}, {0, 0}, {0, 0}, {0, 1} };

	HFONT getHfont(wstring fontFamily, int fontSize, int fontWeight)
	{
		LOGFONT logfont = { fontSize, 0, 0, 0, fontWeight, 0, 0, 0, SHIFTJIS_CHARSET, OUT_TT_ONLY_PRECIS, CLIP_DEFAULT_PRECIS, PROOF_QUALITY, DEFAULT_PITCH | FF_MODERN, L"" };
		lstrcpy(logfont.lfFaceName, fontFamily.c_str());
		HFONT hfont = CreateFontIndirect(&logfont);

		return hfont;
	}

	int getStringAscent(HFONT hfont)
	{
		auto oldObject = SelectObject(hdc, hfont);
		
		TEXTMETRIC textMetric;
		GetTextMetrics(hdc, &textMetric);

		SelectObject(hdc, oldObject);

		return textMetric.tmAscent;
	}

	int getStringHeight(HFONT hfont)
	{
		auto oldObject = SelectObject(hdc, hfont);

		TEXTMETRIC textMetric;
		GetTextMetrics(hdc, &textMetric);

		SelectObject(hdc, oldObject);

		return textMetric.tmHeight;
	}

	int getStringWidth(wstring string, HFONT hfont, UINT antialiasFormat)
	{
		auto oldObject = SelectObject(hdc, hfont);

		size_t width = 0;

		const size_t length = string.length();
		for (int i = 0; i < length; i++)
		{
			GLYPHMETRICS glyphMetrics;
			size_t size = GetGlyphOutline(hdc, string[i], antialiasFormat, &glyphMetrics, 0, NULL, &mat);
			width += glyphMetrics.gmCellIncX;
		}

		SelectObject(hdc, oldObject);

		return width;
	}

	void writeStringToDots(BYTE dots[], wstring string, HFONT hfont, UINT antialiasFormat, COLOR64 fontColor, COLOR64 backgroundColor, int x, int y, int dotWidth, int dotHeight)
	{
		auto oldObject = SelectObject(hdc, hfont);

		const BYTE A = (BYTE)(fontColor >> 24);
		const BYTE R = (BYTE)(fontColor >> 16);
		const BYTE G = (BYTE)(fontColor >> 8);
		const BYTE B = (BYTE)fontColor;

		const BYTE BG_R = (BYTE)(backgroundColor >> 16);
		const BYTE BG_G = (BYTE)(backgroundColor >> 8);
		const BYTE BG_B = (BYTE)backgroundColor;

		int gradationBitCount;
		switch (antialiasFormat)
		{
		case GGO_BITMAP: gradationBitCount = 1; break;
		case GGO_GRAY2_BITMAP: gradationBitCount = 2; break;
		case GGO_GRAY4_BITMAP: gradationBitCount = 4; break;
		case GGO_GRAY8_BITMAP: gradationBitCount = 6; break;
		default: throw invalid_argument("Invalid antialias format.");
		}

		int startingDotX = x;

		TEXTMETRIC textMetric;
		GetTextMetrics(hdc, &textMetric);

		const size_t length = string.length();
		for (int i = 0; i < length; i++)
		{
			GLYPHMETRICS glyphMetrics;
			DWORD size = GetGlyphOutline(hdc, string[i], antialiasFormat, &glyphMetrics, 0, NULL, &mat);
			BYTE* bitmap = new BYTE[size];
			GetGlyphOutline(hdc, string[i], antialiasFormat, &glyphMetrics, size, bitmap, &mat);

			if (backgroundColor != NULL)
			{
				for (int currentDotY = y; currentDotY < y + textMetric.tmHeight; currentDotY++)
				{
					if (currentDotY < 0) continue;
					if (dotHeight <= currentDotY) break;

					for (int currentDotX = startingDotX; currentDotX < startingDotX + glyphMetrics.gmCellIncX; currentDotX++)
					{
						if (currentDotX < 0) continue;
						if (dotWidth <= currentDotX) break;

						const int position_dots = 3 * (currentDotX + dotWidth * currentDotY);

						dots[position_dots + 2] = BG_R;
						dots[position_dots + 1] = BG_G;
						dots[position_dots] = BG_B;
					}
				}
			}

			startingDotX += glyphMetrics.gmptGlyphOrigin.x;

			for (int currentY = 0; currentY < glyphMetrics.gmBlackBoxY; currentY++)
			{
				int currentDotX = startingDotX;
				int currentDotY = y + textMetric.tmAscent - glyphMetrics.gmptGlyphOrigin.y + currentY;

				if (currentDotY < 0) continue;
				if (dotHeight <= currentDotY) break;

				for (int currentX = 0; currentX < glyphMetrics.gmBlackBoxX; currentX++)
				{
					currentDotX++;

					if (currentDotX < 0) continue;
					if (dotWidth <= currentDotX) break;

					const int position_dots = 3 * (currentDotX + dotWidth * currentDotY); // [byte]

					if (gradationBitCount == 1)
					{
						const int stride = (glyphMetrics.gmBlackBoxX + 31) >> 5 << 5;
						const int position_bitmap = currentX + stride * currentY;

						const BYTE currentByte = bitmap[position_bitmap >> 3];
						const BYTE currentBitIndex = position_bitmap - (position_bitmap >> 3 << 3);

						const bool currentBit = (currentByte >> (7 - currentBitIndex)) & 0b1;

						if (currentBit)
						{
							BYTE r = R;
							BYTE g = G;
							BYTE b = B;
							if (A != 0xff)
							{
								r = BG_R + ((int)R - BG_R) * A / 255;
								g = BG_G + ((int)G - BG_G) * A / 255;
								b = BG_B + ((int)B - BG_B) * A / 255;
							}
							dots[position_dots + 2] = r;
							dots[position_dots + 1] = g;
							dots[position_dots] = b;
						}
						else
						{
							dots[position_dots + 2] = BG_R;
							dots[position_dots + 1] = BG_G;
							dots[position_dots] = BG_B;
						}
					}
					else
					{
						const int stride = (glyphMetrics.gmBlackBoxX + 3) >> 2 << 2;

						const BYTE currentBit = bitmap[currentX + stride * currentY];

						BYTE r = R;
						BYTE g = G;
						BYTE b = B;
						if (A != 0xff)
						{
							r = BG_R + ((int)R - BG_R) * A / 255;
							g = BG_G + ((int)G - BG_G) * A / 255;
							b = BG_B + ((int)B - BG_B) * A / 255;
						}
						dots[position_dots + 2] = (r * currentBit) >> gradationBitCount;
						dots[position_dots + 1] = (g * currentBit) >> gradationBitCount;
						dots[position_dots] = (b * currentBit) >> gradationBitCount;
					}

					if (dots[position_dots] < LEDDOT_DEFAULT_COLOR && dots[position_dots + 1] < LEDDOT_DEFAULT_COLOR && dots[position_dots + 2] < LEDDOT_DEFAULT_COLOR)
					{
						dots[position_dots + 2] = LEDDOT_DEFAULT_COLOR;
						dots[position_dots + 1] = LEDDOT_DEFAULT_COLOR;
						dots[position_dots] = LEDDOT_DEFAULT_COLOR;
					}
				}
			}

			startingDotX += glyphMetrics.gmCellIncX - glyphMetrics.gmptGlyphOrigin.x;

			delete[] bitmap;
		}

		SelectObject(hdc, oldObject);
	}
}