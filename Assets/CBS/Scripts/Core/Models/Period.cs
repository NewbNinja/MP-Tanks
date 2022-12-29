using System;
using System.Collections;
using System.Collections.Generic;

namespace CBS
{
    public class Period
    {
        public DateTime Start = DateTime.Now;
        public DateTime End = DateTime.Now.AddMonths(1);
    }
}
