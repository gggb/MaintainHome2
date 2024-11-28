using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Database;

namespace MaintainHome.Models
{
    public class TaskActivity : BaseEntity
    {
        private readonly TaskActivityRepository? _taskActivityRepository;

        public TaskActivity() { }

        public TaskActivity(TaskActivityRepository taskActivityRepository)
        {
            _taskActivityRepository = taskActivityRepository ?? throw new ArgumentNullException(nameof(taskActivityRepository));
        }

        [PrimaryKey, AutoIncrement] public override int Id { get; set; }
        [Indexed] public int TaskId { get; set; }
        public string? Status { get; set; }
        public string? Condition { get; set; }
        public string? Action { get; set; }
        public decimal TimeSpent { get; set; }
        public string? Notes { get; set; }

        public override async Task<bool> Add()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = CreatedDate;

            if (_taskActivityRepository == null)
            {
                throw new InvalidOperationException("TaskActivityRepository is not initialized.");
            }
            return await _taskActivityRepository.AddTaskActivity(this);
        }

        public override async Task<bool> Get(int id)
        {
            if (_taskActivityRepository == null)
            {
                throw new InvalidOperationException("TaskActivityRepository is not initialized.");
            }

            var taskActivity = await _taskActivityRepository.GetTaskActivityAsync(id);
            if (taskActivity != null)
            {
                this.Id = taskActivity.Id;
                this.CreatedDate = taskActivity.CreatedDate;
                this.ModifiedDate = taskActivity.ModifiedDate;
                this.TaskId = taskActivity.TaskId;
                this.Status = taskActivity.Status;
                this.Condition = taskActivity.Condition;
                this.Action = taskActivity.Action;
                this.TimeSpent = taskActivity.TimeSpent;
                this.Notes = taskActivity.Notes;
                return true;
            }
            return false;
        }

        public override async Task<bool> Update()
        {
            ModifiedDate = DateTime.Now;
            if (_taskActivityRepository == null)
            {
                throw new InvalidOperationException("TaskActivityRepository is not initialized.");
            }
            return await _taskActivityRepository.UpdateTaskActivityAsync(this);
        }

        public override async Task<bool> Delete(int id)
        {
            if (_taskActivityRepository == null)
            {
                throw new InvalidOperationException("TaskActivityRepository is not initialized.");
            }
            return await _taskActivityRepository.DeleteTaskActivityAsync(id);
        }
    }
}

