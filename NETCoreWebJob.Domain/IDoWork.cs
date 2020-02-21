using NETCoreWebJob.Domain.Models.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NETCoreWebJob.Domain
{
    public interface IDoWork
    {
        Task<MyDBModelDTO> IDoStuff(int id);
        Task SaveMyEntity(MyDBModelDTO myEntity);
    }
}
