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

        string lpNumEnglish { set; get; }
        string lpNumArabic { set; get; }
        string recordingPath { set; get; }
        
        Bitmap stitchImage { set; }
        Bitmap comparisonImage { set; }
        
        void BindData(DataTable dt);
        void Save();
    }
}
