Dynamic-EntityFrameWork
=======================

C# Library for dynamic CRUD operations using Entity Frame Work and  Connected database access

Introduction:

If you are tired of creating read/write action web services for all and each ADO.NET Entity Data Model you have in your solution? 
And want to have a method that takes any dynamic entity and throws it in your database?
Or to have a collection of methods that delete, update, get and find any entity you want from the database?
Then this library is made for you! :)

Example:

public class EFCRUDController : ApiController
    {
        static kasemEntities mCompanyEntities = new kasemEntities();
        static string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        static string nameSpace = assemblyName + ".Models";
        EntityFrameworkCRUD crud = new EntityFrameworkCRUD(assemblyName, nameSpace, mCompanyEntities);

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
        public object update(string tablename, dynamic entity)
        {
            return crud.update(tablename, entity);
        }

        [HttpPost, ActionName("updateAll")]
        public object updateAll(string tablename, dynamic lEntity)
        {
            return crud.updateAll(tablename, lEntity);
        }

        [HttpPost, ActionName("subQuery")]
        public object subQuery(KasCRUD.EntityFrameworkCRUD.subQueryObject sqo )
        {
            return crud.subQuery(sqo);
        }
    }
