using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using KasCRUD;

namespace KasCRUDSampleTest.Controllers
{
    public class CCRUDController : ApiController
    {
        static SqlConnection mSqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ToString());
        static ConnectedCRUD crud = new ConnectedCRUD(mSqlConnection);

        [HttpGet, ActionName("getAll")]
        public object getAll(string tablename)
        {
            return crud.getAll(tablename);
        }

        [HttpGet, ActionName("getSingle")]
        public object getSingle(string tablename, int Id)
        {
            return crud.getSingle(tablename, Id);
        }

        [HttpGet, ActionName("delete")]
        public object delete(string tablename, int Id)
        {
            return crud.delete(tablename, Id);
        }

        [HttpPost, ActionName("addAll")]
        public object addAll(string tablename, dynamic lEntity)
        {
            return crud.addAll(tablename, lEntity);
        }

        [HttpPost, ActionName("add")]
        public object add(string tablename, dynamic entity)
        {
            return crud.add(tablename, entity);
        }

        [HttpPost, ActionName("findAll")]
        public object findAll(string tablename, Dictionary<string, string> field)
        {
            return crud.findAll(tablename, field);
        }

        [HttpPost, ActionName("update")]
        public object c(string tablename, dynamic entity)
        {
            return crud.update(tablename, entity);
        }

        [HttpPost, ActionName("updateAll")]
        public object updateAll(string tablename, dynamic lEntity)
        {
           dynamic asd=  crud.updateAll(tablename, lEntity);
           return asd;
        }

    }
}
