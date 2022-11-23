using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;

namespace MechaChat.API.Handlers
{
    public class MySQLHandler
    {
        private static string _connectionString;

        private static string ConnectionString()
        {
            MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder
            {
                ApplicationName = "MechaChat_API",
                Database = "",
                Server = "",
                Port = 3306,
                UserID = "",
                Password = ""
            };

            return mySqlConnectionStringBuilder.ToString();
        }

        public static async Task<List<T>> SelectListQuery<T>(string Sql, DynamicParameters args = null)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                var map = new CustomPropertyTypeMap(typeof(T), (type, columnName) => type.GetProperties().FirstOrDefault(prop => GetDescriptionFromAttribute(prop, columnName) == columnName));
                SqlMapper.SetTypeMap(typeof(T), map);

                using (MySqlConnection conn = new MySqlConnection(ConnectionString()))
                {
                    return (await conn.QueryAsync<T>(Sql, args)).AsList();
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler(Sql, ex.Message, watch.ElapsedMilliseconds);
            }
            finally
            {
                watch.Stop();
            }

            return null;
        }

        public static async Task<T> SelectSingleQuery<T>(string Sql, DynamicParameters args = null)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                var map = new CustomPropertyTypeMap(typeof(T), (type, columnName) => type.GetProperties().FirstOrDefault(prop => GetDescriptionFromAttribute(prop, columnName) == columnName));
                SqlMapper.SetTypeMap(typeof(T), map);

                using (MySqlConnection conn = new MySqlConnection(ConnectionString()))
                {
                    return (await conn.QueryAsync<T>(Sql, args)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler(Sql, ex.Message, watch.ElapsedMilliseconds);
            }
            finally
            {
                watch.Stop();
            }

            return default(T);
        }

        public static async Task<bool> ExecuteQuery(string Sql, DynamicParameters args = null)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                using (var conn = new MySqlConnection(ConnectionString()))
                {
                    return (await conn.ExecuteAsync(Sql, args)) > 0;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler(Sql, ex.Message, watch.ElapsedMilliseconds);
            }
            finally
            {
                watch.Stop();
            }

            return false;
        }

        private static void ExceptionHandler(string query, string exceptionMessage, long elapsedMilliseconds)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("** SQL Exception **\n");
            sb.Append($"Query: {query}\n");
            sb.Append($"Exception Message: {exceptionMessage}\n");
            sb.Append($"Time Elapsed: {elapsedMilliseconds}ms");

            Console.WriteLine($"{sb}");
        }

        public static string GetDescriptionFromAttribute(MemberInfo member, string columnName)
		{
			if (member == null) return null;

			var attrib = (DescriptionAttribute)Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute), false);

			return (attrib?.Description ?? member.Name);
		}
    }
}
