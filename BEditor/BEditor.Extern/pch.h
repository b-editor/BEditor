// pch.h: �v���R���p�C���ς݃w�b�_�[ �t�@�C���ł��B
// ���̃t�@�C���́A���̌�̃r���h�̃r���h �p�t�H�[�}���X�����コ���邽�� 1 �񂾂��R���p�C������܂��B
// �R�[�h�⊮�⑽���̃R�[�h�Q�Ƌ@�\�Ȃǂ� IntelliSense �p�t�H�[�}���X�ɂ��e�����܂��B
// �������A�����Ɉꗗ�\������Ă���t�@�C���́A�r���h�Ԃł����ꂩ���X�V�����ƁA���ׂĂ��ăR���p�C������܂��B
// �p�ɂɍX�V����t�@�C���������ɒǉ����Ȃ��ł��������B�ǉ�����ƁA�p�t�H�[�}���X��̗��_���Ȃ��Ȃ�܂��B

#ifndef PCH_H
#define PCH_H

// �v���R���p�C������w�b�_�[�������ɒǉ����܂�

#define DLLExport(T) extern "C" __declspec(dllexport) T

#include <opencv2/opencv.hpp>

#include "Color.h"
#include "CvMat.h"

#include "ImageFont.h"

#endif //PCH_H