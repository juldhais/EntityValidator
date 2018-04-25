using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntityValidator;

namespace EntityValidator.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new TestModel();
            DbValidator.Validate(db);

            var newRecord = new TestTable();
            newRecord.Id = Guid.NewGuid();
            newRecord.StringProperty = "";
            newRecord.IntProperty = 0;
            newRecord.DecimalProperty = 0;
            newRecord.DateTimeProperty = DateTime.Now;
            newRecord.GuidProperty = Guid.NewGuid();
            db.TestTables.Add(newRecord);
            db.SaveChanges();

            var list = db.TestTables.ToList();
            foreach (var item in list)
            {
                Console.WriteLine($"Id : {item.Id}");
            }
        }
    }
}
