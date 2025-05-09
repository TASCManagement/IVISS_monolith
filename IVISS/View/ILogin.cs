﻿using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVISS.View
{
    interface ILogin
    {
        string username { set; get; }
        string password { set; get; }

        MaterialForm currentForm { get; set; }
        string errMsg { set; }

        event EventHandler BtnOk;

        void CloseForm();

    }
}
