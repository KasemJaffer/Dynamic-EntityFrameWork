using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace KasCRUD
{
    public class EntityFrameworkCRUD
    {
        private object databaseContext;
        private GenericMethods mGenericMethods;

        /// <summary>
        /// Generic CRUD operations helper using Entity FrameWork
        /// </summary>
        /// <param name="assemblyName">i.e System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;</param>
        /// <param name="namespaceName">The name space of the model classes</param>
        public EntityFrameworkCRUD(string assemblyName, string namespaceName, object databaseContext)
        {
            if (!(typeof(DbContext).IsAssignableFrom(databaseContext.GetType()))) throw new System.ArgumentException("Parameter must be of type DbContext", "databaseContext");
            if (databaseContext == null) throw new System.ArgumentException("Parameter must not be null", "databaseContext");
            if (String.IsNullOrEmpty(namespaceName)) throw new System.ArgumentException("Parameter must not be null", "namespaceName");
            this.mGenericMethods = new GenericMethods(assemblyName, namespaceName);
            this.databaseContext = databaseContext;
        }

        public JToken add(string tableName, dynamic entity)
        {

            //object instance = createInstanceWithEntity(tableName, entity);
            //if (instance.GetType() == typeof(CRUD_Error)) return instance;
            //dynamic returnedObject = invokeMethodFromInstance(entities, tableName, "Add", new object[] { instance });
            //if (returnedObject.GetType() == typeof(CRUD_Error)) return returnedObject;

            //dynamic res = invokeMethodFromInstance(entities, "SaveChanges", null);


            dynamic instance = mGenericMethods.createInstanceWithEntity(tableName, entity);
            if (instance.GetType() == typeof(CRUDError)) return instance;
            dynamic res2 = mGenericMethods.invokeMethodFromInstance(databaseContext, "Entry", new object[] { instance });
            if (instance.Id == 0) res2.State = EntityState.Added;
            dynamic resas = mGenericMethods.invokeMethodFromInstance(databaseContext, "SaveChanges", null);
            var jsonObjects = JsonHelper.ConvertToSimpleObject(res2.Entity);
            JToken mJToken = JToken.FromObject(jsonObjects);
            return mJToken;
        }

        public JArray addAll(string tableName, dynamic lEntity)
        {
            List<dynamic> addedEntities = new List<dynamic>();
            foreach (dynamic entity in lEntity)
            {
                dynamic instance = mGenericMethods.createInstanceWithEntity(tableName, entity);
                if (instance.GetType() == typeof(CRUDError)) return instance;
                dynamic res2 = mGenericMethods.invokeMethodFromInstance(databaseContext, "Entry", new object[] { instance });
                if (instance.Id == 0) res2.State = EntityState.Added;
                addedEntities.Add(res2.Entity);
            }
            dynamic resas = mGenericMethods.invokeMethodFromInstance(databaseContext, "SaveChanges", null);
            var jsonObjects = addedEntities.Select(c => JsonHelper.ConvertToSimpleObject(c));
            JArray mJArray = JArray.FromObject(jsonObjects);
            return mJArray;
        }

        public JToken update(string tableName, dynamic entity)
        {

            dynamic instance = mGenericMethods.createInstanceWithEntity(tableName, entity);
            if (instance.GetType() == typeof(CRUDError)) return instance;
            dynamic res2 = mGenericMethods.invokeMethodFromInstance(databaseContext, "Entry", new object[] { instance });
            if (instance.Id != 0) res2.State = EntityState.Modified;
            dynamic resas = mGenericMethods.invokeMethodFromInstance(databaseContext, "SaveChanges", null);
            var jsonObjects = JsonHelper.ConvertToSimpleObject(res2.Entity);
            JToken mJToken = JToken.FromObject(jsonObjects);
            return mJToken;
        }

        public JArray updateAll(string tableName, dynamic lEntity)
        {
            List<dynamic> updatedEntities = new List<dynamic>();
            foreach (dynamic entity in lEntity)
            {
                dynamic updatedEntity = update(tableName, entity);
                updatedEntities.Add(updatedEntity);
            }

            JArray mJArray = JArray.FromObject(updatedEntities);
            return mJArray;
        }

        public JToken delete(string tableName, int Id)
        {

            dynamic instance = mGenericMethods.createInstance(tableName);
            if (instance.GetType() == typeof(CRUDError)) return instance;
            instance.Id = Id;
            dynamic res2 = mGenericMethods.invokeMethodFromInstance(databaseContext, "Entry", new object[] { instance });
            if (instance.Id != 0) res2.State = EntityState.Deleted;
            dynamic resas = mGenericMethods.invokeMethodFromInstance(databaseContext, "SaveChanges", null);
            var jsonObjects = JsonHelper.ConvertToSimpleObject(res2.Entity);
            JToken mJToken = JToken.FromObject(jsonObjects);
            return mJToken;
        }
       
        public JArray getAll(string tableName)
        {
            dynamic instance = mGenericMethods.getInnerInstanceName(databaseContext, tableName);
            if (instance.GetType() == typeof(CRUDError)) return instance;
            dynamic  jsonObjects=null;
            try
            {
                dynamic returnedValues = instance.SqlQuery("SELECT * FROM " + tableName);
                 jsonObjects = ((IEnumerable<dynamic>)returnedValues).ToList().Select(c => JsonHelper.ConvertToSimpleObject(c));
            }
            catch(Exception e)
            {
                jsonObjects = ((IEnumerable<dynamic>)instance).ToList().Select(c => JsonHelper.ConvertToSimpleObject(c));
            }
            JArray mJArray = JArray.FromObject(jsonObjects);
            return mJArray;
        }

        public JToken getSingle(string tableName, int Id)
        {
            dynamic instance = mGenericMethods.getInnerInstanceName(databaseContext, tableName);
            if (instance.GetType() == typeof(CRUDError)) return instance;
            dynamic returnedObject = instance.Find(Id);
            var jsonObjects = JsonHelper.ConvertToSimpleObject(returnedObject);
            JToken jToken = JToken.FromObject(jsonObjects);
            return jToken;
        }

        public JArray findAll(string tableName, Dictionary<string, string> fields)
        {
            StringBuilder queryString = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in fields)
            {
                if (queryString.Length > 0)
                    queryString.AppendFormat(" and {0}='{1}'", kvp.Key, kvp.Value);
                else
                    queryString.AppendFormat("{0}='{1}'", kvp.Key, kvp.Value);
            }
            dynamic innerInstance = mGenericMethods.getInnerInstanceName(databaseContext, tableName);
            if (innerInstance.GetType() == typeof(CRUDError)) return innerInstance;
            dynamic returnedValues = innerInstance.SqlQuery("SELECT * FROM " + tableName + " WHERE " + queryString.ToString());
            var jsonObjects = ((IEnumerable<dynamic>)returnedValues).ToList().Select(c => JsonHelper.ConvertToSimpleObject(c));

            JArray jArray = JArray.FromObject(jsonObjects);

            return jArray;
        }

        public JArray subQuery(subQueryObject sqo)
        {
            string SqlCommand = String.Format("select * from {0} where {0}.[{1}] = (select {3}.{2} from {3} where {3}.{4}='{5}')", sqo.foriegnTable, sqo.foriegnKey, sqo.primaryKey, sqo.primaryTable, sqo.primaryKey, sqo.value);
            Type type = (Type)mGenericMethods.getType(sqo.foriegnTable);
            object[] mParams = new object[] { type, SqlCommand.ToString() };
            dynamic instance = mGenericMethods.getInnerInstanceName(databaseContext, "Database");
            var returnedObject = ((Database)instance).SqlQuery(type, SqlCommand).Cast<dynamic>().ToList();
            var jsonObjects = returnedObject.Select(c => JsonHelper.ConvertToSimpleObject(c));
            JArray mJArray = JArray.FromObject(jsonObjects);
            return mJArray;
        }

        public class subQueryObject
        {
            public string foriegnTable { set; get; }
            public string foriegnKey { set; get; }
            public string primaryKey { set; get; }
            public string primaryTable { set; get; }
            public int value { set; get; }
        }

    }

}