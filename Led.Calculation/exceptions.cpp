#include "stdafx.h"

using namespace std;

namespace led_drawing_calculator
{
	// �w�肳�ꂽ�t�H���g�t�@�~���[��������Ȃ��������Ƃ�������O�ł��B
	class FontFamilyNotFoundException : public invalid_argument {};

	// �����ȃA���`�G�C���A�X�t�H�[�}�b�g���w�肳�ꂽ���Ƃ�������O�ł��B
	class InvalidAntialiasFormatException : public invalid_argument {};
}