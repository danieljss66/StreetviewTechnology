#include <iostream>
#include <Windows.h>
#include <opencv2/opencv.hpp>
 
using namespace std;
using namespace cv;
 
int main (int arg, char * argv[])
{

    char * val = "";

	if (arg > 1)
	{
		val = argv[1];
	}
	else
	{
		//char szFile[100];
		//OPENFILENAME ofn;
		//ZeroMemory( &ofn , sizeof( ofn));
		//ofn.lStructSize = sizeof ( ofn );
		//ofn.hwndOwner = NULL  ;
		//wchar_t wFile[100];
		//mbstowcs(wFile, szFile, strlen(szFile) + 1);
		//ofn.lpstrFile = wFile;
		//ofn.lpstrFile[0] = L'\0';
		//ofn.nMaxFile = sizeof( szFile );
		//ofn.lpstrFilter = L"All\0*.*\0Text\0*.TXT\0";
		//ofn.nFilterIndex =1;
		//ofn.lpstrFileTitle = NULL ;
		//ofn.nMaxFileTitle = 0 ;
		//ofn.lpstrInitialDir=NULL ;
		//ofn.Flags = OFN_PATHMUSTEXIST|OFN_FILEMUSTEXIST ;
		//GetOpenFileName( &ofn );

		//val = new char[wcslen(ofn.lpstrFile) + 1];
		//wsprintfA(val, "%S", ofn.lpstrFile);
	}

	try
	{
		IplImage *image = cvLoadImage(val); //"f:\\test\\11.jpg"
		Mat img(image);
		HOGDescriptor hog;
		hog.setSVMDetector(HOGDescriptor::getDefaultPeopleDetector());
 
 
		vector<Rect> found, found_filtered;
		hog.detectMultiScale(img, found, 0, Size(8,8), Size(32,32), 1.05, 2);
		//hog.detectMultiScale(img, found, 0, Size(6,6), Size(32,32), 1.05, 0, true);
 
		size_t i, j;
		for (i=0; i<found.size(); i++)
		{
			Rect r = found[i];
			for (j=0; j<found.size(); j++)
				if (j!=i && (r & found[j])==r)
					break;
			if (j==found.size())
				found_filtered.push_back(r);
		}
		for (i=0; i<found_filtered.size(); i++)
		{
			Rect r = found_filtered[i];

			r.x += cvRound(r.width*0.1);
			r.width = cvRound(r.width*0.8);
			//r.y += cvRound(r.height*0.06);
			r.height = cvRound(r.height*0.9);
			rectangle(img, r.tl(), r.br(), cv::Scalar(0,0,255), 1);
		}

		namedWindow("Image Viewer", CV_WINDOW_AUTOSIZE);
		imshow("Image Viewer", img);
		waitKey(0);
	}
	catch (exception e)
	{

	}

    return 0;
}