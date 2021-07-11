#pragma once
#include "pch.h"

typedef	struct VideoStreamInfo
{
	// �R�[�f�b�N��
	const char* codec;
	// �b��
	double duration;
	// ��
	int width;
	// ����
	int height;
	// �t���[����
	int framenum;
	// �t���[�����[�g
	int framerate;
} VideoStreamInfo;

class VideoStream
{
public:
	VideoStreamInfo info;

	VideoStream(IMFSourceReader* reader);
	~VideoStream();

	int TryGetFrame(long position, Image* image);
private:
	IMFSourceReader* reader;
	long duration;
	long current;
};