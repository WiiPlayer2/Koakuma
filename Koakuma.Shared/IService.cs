﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public interface IService : IModule
    {
        void Start();
        void Stop();
    }
}