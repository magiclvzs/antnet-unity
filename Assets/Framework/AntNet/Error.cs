using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntNet
{
    class Error
    {
         
        public static ushort  ErrOk = 0;            //正确
        public static ushort  ErrJsonPack = 8;
        public static ushort  ErrJsonUnPack = 9;
        public static ushort  ErrNetTimeout = 200;     //网络超时  
       
    }
}
