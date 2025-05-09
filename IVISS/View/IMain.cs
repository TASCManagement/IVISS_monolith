using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

namespace IVISS.View
{
    interface IMain
    {
        event EventHandler BtnLights;
        event EventHandler BtnAirClean;
        event EventHandler BtnCamera;
        event EventHandler BtnSearchRecords;

        event EventHandler BtnRecordingOn;
        event EventHandler BtnRecordingOff;

        event EventHandler BtnSettings;

        event EventHandler FormLoaded;
        event EventHandler FormIsClosing;

        string lpNumEnglish { set; get; }
        string lpNumArabic { set; get; }
        string accuracy { set; get; }
        string origin { set; get; }
        string plateColor { set; get; }
        string plateSubColor { set; get; }
        string recordingPath { set; get; }
        bool auto { set; get; }

        Bitmap stitchImage { set; }
        Bitmap comparisonImage { set; }
        
        void BindData(DataTable dt);
        void LoadCompositeImage(string destDir);
        void LoadImageComparison(string destDir);

      //  void LoadImageComparisonWithFOD(string destDir);

        
        bool StartRecording();
        bool StopRecording();
         void RunFODAsync(string caseimage, string referenceimage,bool IsManual);
        //void Save();
    }
}
