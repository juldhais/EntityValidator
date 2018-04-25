namespace EntityValidator.Test
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;

    public class TestModel : DbContext
    {
        public TestModel()
            : base("name=TestModel")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            DbValidator.CreateModelBuilder(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<TestTable> TestTables { get; set; }
    }

    public class TestTable
    {
        public Guid Id { get; set; }
        public string StringProperty { get; set; }
        public int? IntProperty { get; set; }
        public decimal? DecimalProperty { get; set; }
        public DateTime? DateTimeProperty { get; set; }
        public Guid? GuidProperty { get; set; }
    }
}