using NETCoreWebJob.Domain.Models.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NETCoreWebJob.Domain
{
    public class DoWork : IDoWork
    {
        private readonly IMyDBContext _context;
        public DoWork(IMyDBContext context)
        {
            _context = context;
        }
        public async Task<MyDBModelDTO> IDoStuff(int i)
        {
            var result = await _context.GetById(i);
            return new MyDBModelDTO
            {
                ModelId = result.Id,
                Name = result.Name
            };
        }

        public async Task SaveMyEntity(MyDBModelDTO myEntity)
        {
            await Task.Delay(0);
        }
    }
}
