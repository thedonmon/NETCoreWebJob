using Microsoft.EntityFrameworkCore;
using NETCoreWebJob.Domain.Models;
using NETCoreWebJob.Domain.Models.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NETCoreWebJob.Domain
{
    public class MyDBContext : DbContext, IMyDBContext
    {
        public DbSet<MyDBModel> MyTableName { get; set; }
        public MyDBContext(DbContextOptions<MyDBContext> options): base(options)
        {

        }
        public async Task<MyDBModel> GetById(int id)
        {
            return await MyTableName.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
    public interface IMyDBContext
    {
        DbSet<MyDBModel> MyTableName { get; set; }
        Task<MyDBModel> GetById(int id);
    }
}
