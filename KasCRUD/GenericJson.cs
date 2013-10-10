using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KasCRUD
{
    public class GenericJson
    {
        public string Entity;
        public List<Methods> Methods;
    }

    public class Methods
    {
       public string Method;
       public List<Pres> Pre;
       public List<Pres> Post;
       public object[] param;
    }

    public class Pres
    { 
        public string LipPath;
        public string Method;
        public string Entity;
        public object[] param;
    }
}