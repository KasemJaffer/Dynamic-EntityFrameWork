using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Text;

namespace KasCRUD
{
    public class ConnectedCRUD
    {
        private SqlConnection mSqlConnection;
        private SqlDataAdapter mSqlDataAdapter;
        private List<string> tables;


        //Parameters:
        //  SqlConnection:
        //    i.e  new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionStringName"].ToString());
        public ConnectedCRUD(SqlConnection mSqlConnection)
        {
            this.mSqlConnection = mSqlConnection;
            this.mSqlDataAdapter = new SqlDataAdapter();
            this.tables = new List<string>();

            mSqlConnection.Open();
            DataTable mDataTable = mSqlConnection.GetSchema("Tables");
            mSqlConnection.Close();
            foreach (DataRow row in mDataTable.Rows)
            {
                string tablename = (string)row[2];
                this.tables.Add(tablename);
            }
        }
        
        public object add(string tableName, dynamic entity)
        {
            if (!isTableExist(tableName)) return new { Error = "Table not found" };
            
            JToken obj = JToken.FromObject(entity);
            string commandText = "INSERT INTO " + tableName + " (";
            foreach (dynamic entry in obj)
            {
                string name = entry.Name; // "test"
                commandText += name + ", ";
            }
            commandText = commandText.Substring(0, commandText.Length - 2);
            commandText += ") OUTPUT INSERTED.* VALUES (";
            foreach (dynamic entry in obj)
            {
                dynamic value = entry.Value; // { inner: "text-value" }
                commandText += "'" + value + "'" + ", ";
            }
            commandText = commandText.Substring(0, commandText.Length - 2);
            commandText += ")";

            SqlCommand mSqlCommand = new SqlCommand();
            mSqlCommand.CommandText = commandText;
            mSqlCommand.Connection = mSqlConnection;

            int response;
            try
            {
                mSqlConnection.Open();
                response = (int)mSqlCommand.ExecuteScalar();
                mSqlConnection.Close();
            }
            catch (Exception e)
            {
                mSqlConnection.Close();
                return e.Message;
            }
            return response;
        }

        public object addAll(string tableName, dynamic lentity)
        {
            if (!isTableExist(tableName)) return new { Error = "Table not found" };
            
            JArray lObj = JArray.FromObject(lentity);
            List<int> insertedIds = new List<int>();
            foreach (dynamic obj in lObj)
            {
                string commandText = "INSERT INTO " + tableName + " (";
                foreach (dynamic entry in obj)
                {
                    string name = entry.Name; 
                    commandText += name + ", ";
                }
                commandText = commandText.Substring(0, commandText.Length - 2);
                commandText += ") OUTPUT INSERTED.* VALUES (";
                foreach (dynamic entry in obj)
                {
                    dynamic value = entry.Value; 
                    commandText += "'" + value + "'" + ", ";
                }
                commandText = commandText.Substring(0, commandText.Length - 2);
                commandText += ")";

                SqlCommand mSqlCommand = new SqlCommand();
                mSqlCommand.CommandText = commandText;
                mSqlCommand.Connection = mSqlConnection;

                int response;
                try
                {
                    mSqlConnection.Open();
                    response = (int)mSqlCommand.ExecuteScalar();
                    mSqlConnection.Close();

                    insertedIds.Add(response);
                }
                catch (Exception e)
                {
                    mSqlConnection.Close();
                    insertedIds.Add(0);
                }
            }
            return insertedIds;
        }

        public object update(string tableName, dynamic entity)
        {
            if (!isTableExist(tableName)) return new { Error = "Table not found" };

            JToken obj = JToken.FromObject(entity);
            string commandText = "UPDATE " + tableName + " SET ";
            string IdName="", IdValue="";
            foreach (dynamic entry in obj)
            {             
                string name = entry.Name; 
                string value = entry.Value;
                if (name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    IdName = name;
                    IdValue = value;
                }else commandText += name + "='"+ value + "', ";
            }
            commandText = commandText.Substring(0, commandText.Length - 2);
            commandText += "OUTPUT INSERTED.* WHERE " + IdName + "='" + IdValue + "'";


            SqlCommand mSqlCommand = new SqlCommand();
            mSqlCommand.CommandText = commandText;
            mSqlCommand.Connection = mSqlConnection;

            dynamic response;
            try
            {
                mSqlConnection.Open();
                response = mSqlCommand.ExecuteScalar();
                mSqlConnection.Close();
            }
            catch (Exception e)
            {
                mSqlConnection.Close();
                return e.Message;
            }
            return response;
        }

        public object updateAll(string tableName, dynamic lEntity)
        {
            if (!isTableExist(tableName)) return new { Error = "Table not found" };

            List<dynamic> updatedEntities = new List<dynamic>();
            foreach (dynamic entity in lEntity)
            {
                updatedEntities.Add(update(tableName, entity));
            }
            return updatedEntities;
        }
  
        /// <summary>
        /// Gets all the records of the table
        /// </summary>
        /// <param name="tableName">Table name as is in the database</param>
        /// <returns>Returns JArray in object</returns>
        /// <example >i.e ((JArray)crud.getAll(tableName)).ToObject();</example>
        public object getAll(string tableName)
        {
            if (!isTableExist(tableName)) return new { Error = "Table not found" };

            SqlCommand mSqlCommand = new SqlCommand();
            mSqlCommand.CommandText = "SELECT * FROM " + tableName;
            mSqlCommand.Connection = mSqlConnection;
            DataTable dt = new DataTable();
            mSqlDataAdapter.SelectCommand = mSqlCommand;          
            try
            {
                mSqlDataAdapter.Fill(dt);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            string serialized = JsonConvert.SerializeObject(dt);
            JArray deSerialized = JsonConvert.DeserializeObject<JArray>(serialized);
            return deSerialized;
        }
 
        /// <summary>
        /// Gets a record from table in JToken object
        /// </summary>
        /// <param name="tableName"> Table name as is in the database</param>
        /// <param name="Id"> Record Id to get</param>
        /// <returns>Returns JToken in Object</returns>
        /// <example > ((JToken)crud.getSingle(tableName, Id)).ToObject();</example>
        public object getSingle(string tableName, int Id)
        {
            if (!isTableExist(tableName)) return new { Error = "Table not found" };

            SqlCommand mSqlCommand = new SqlCommand();
            mSqlCommand.CommandText = "SELECT * FROM " + tableName + " WHERE Id=" + Id;
            mSqlCommand.Connection = mSqlConnection;
            DataTable dt = new DataTable();
            mSqlDataAdapter.SelectCommand = mSqlCommand;
            try
            {
                mSqlDataAdapter.Fill(dt);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            string serialized = JsonConvert.SerializeObject(dt);
            JArray deSerialized = JsonConvert.DeserializeObject<JArray>(serialized);
            JToken token = deSerialized.First;
            return deSerialized.First;
        }

        public object delete(string tableName, int Id)
        {
            if (!isTableExist(tableName)) return new { Error = "Table not found" };
            SqlCommand mSqlCommand = new SqlCommand();
            mSqlCommand.CommandText = "DELETE FROM " + tableName + "OUTPUT DELETED.* WHERE Id=" + Id;
            mSqlCommand.Connection = mSqlConnection;

            int response;
            try
            {
                mSqlConnection.Open();
                response = mSqlCommand.ExecuteNonQuery();
                mSqlConnection.Close();
            }
            catch (Exception e)
            {
                mSqlConnection.Close();
                return e.Message;
            }

            return response;
        }

        public object findAll(string tableName, Dictionary<string,string> fields)
        {
            if (!isTableExist(tableName)) return new { Error = "Table not found" };
            
            StringBuilder queryString = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in fields)
            {
                if (queryString.Length > 0)
                    queryString.AppendFormat(" and {0}='{1}'", kvp.Key, kvp.Value);
                else
                    queryString.AppendFormat("{0}='{1}'", kvp.Key, kvp.Value);
            }

            SqlCommand mSqlCommand = new SqlCommand();
            mSqlCommand.CommandText = "SELECT * FROM " + tableName + " WHERE " + queryString.ToString();
            mSqlCommand.Connection = mSqlConnection;
            DataTable dt = new DataTable();
            mSqlDataAdapter.SelectCommand = mSqlCommand;
            try
            {
                mSqlDataAdapter.Fill(dt);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            string serialized = JsonConvert.SerializeObject(dt);
            JArray deSerialized = JsonConvert.DeserializeObject<JArray>(serialized);
            return deSerialized;
        }

        public bool isTableExist(string tableToCheck)
        {
            bool isFound = false;
            foreach (string tablename in this.tables)
            {
                if (tableToCheck.Equals(tablename, StringComparison.OrdinalIgnoreCase))
                {
                    isFound = true;
                    break;
                }
            }
            if (isFound) return true;
            else return false;
        }

    }
}