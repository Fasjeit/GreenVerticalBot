﻿using GreenVerticalBot.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenVerticalBot.EntityFramework.Store
{
    internal class StubTaskStore : ITasksStore
    {
        public GreenVerticalBotContext Context => throw new NotImplementedException();

        public Task AddTaskAsync(TaskEntity entity)
        {
            return Task.CompletedTask;
        }
    }
}
