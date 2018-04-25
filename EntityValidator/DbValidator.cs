using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Reflection;

namespace EntityValidator
{
    public static class DbValidator
    {
        public static void Validate<T>(T dbContext) where T : DbContext
        {
            if (dbContext.Database.Exists())
            {
                Database.SetInitializer<T>(null);

                var listModel = GetListModelType(dbContext);
                foreach (var model in listModel)
                {
                    var tableName = GetTableName(model);
                    CreateTable(dbContext, tableName);

                    var properties = model.GetProperties();
                    foreach (var propInfo in properties)
                        CreateColumn(dbContext, tableName, propInfo);
                }
            }
            else
            {
                dbContext.Database.Create();
            }
        }

        public static DbModelBuilder CreateModelBuilder(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Properties<decimal>().Configure(c => c.HasPrecision(25, 10));
            return modelBuilder;
        }

        private static List<Type> GetListModelType(DbContext dbContext)
        {
            var result = new List<Type>();
            
            var properties = dbContext.GetType().GetProperties();

            foreach (var item in properties)
            {
                var propertyType = item.PropertyType;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var modelType = propertyType.GetGenericArguments()[0];
                    result.Add(modelType);
                }
            }

            return result;
        }

        private static string GetTableName(Type modelType)
        {
            var result = "";

            result = modelType.Name;

            return result;
        }

        private static void CreateTable(DbContext dbContext, string tableName)
        {
            try
            {
                string query = 
                    string.Format(
                        @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'{0}' AND type = 'U')
                            CREATE TABLE [{0}](
	                        [Id] [uniqueidentifier] PRIMARY KEY NOT NULL
                            )"
                    , tableName);

                dbContext.Database.ExecuteSqlCommand(query);
            }
            catch { }
        }

        private static void CreateColumn(DbContext dbContext, string tableName, PropertyInfo propInfo)
        {
            try
            {
                var typeDictionary = new Dictionary<Type, string>();
                typeDictionary.Add(typeof(Guid), "uniqueidentifier");
                typeDictionary.Add(typeof(Guid?), "uniqueidentifier");
                typeDictionary.Add(typeof(string), "nvarchar(max)");
                typeDictionary.Add(typeof(int), "int");
                typeDictionary.Add(typeof(int?), "int");
                typeDictionary.Add(typeof(decimal), "decimal(25,10)");
                typeDictionary.Add(typeof(decimal?), "decimal(25,10)");
                typeDictionary.Add(typeof(DateTime), "datetime");
                typeDictionary.Add(typeof(DateTime?), "datetime");
                typeDictionary.Add(typeof(bool), "bit");
                typeDictionary.Add(typeof(bool?), "bit");

                if (!typeDictionary.ContainsKey(propInfo.PropertyType)) return;

                var columnName = propInfo.Name;
                var columnType = typeDictionary[propInfo.PropertyType];

                string query = string.Format(@"IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'{1}' AND Object_ID = Object_ID(N'{0}'))
                                        BEGIN
                                            ALTER TABLE [{0}]
                                            ADD [{1}] {2}
                                        END", tableName, columnName, columnType);

                dbContext.Database.ExecuteSqlCommand(query);
            }
            catch { }
        }
    }
}
