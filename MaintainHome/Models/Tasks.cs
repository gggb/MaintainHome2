using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Database;

namespace MaintainHome.Models
{
    public class Tasks : BaseEntity
    {
        private readonly TasksRepository? _tasksRepository;

        public Tasks() { }   // Parameterless constructor
        public Tasks(TasksRepository tasksRepository)
        {
            _tasksRepository = tasksRepository ?? throw new ArgumentNullException(nameof(tasksRepository));
        }

        [PrimaryKey, AutoIncrement] public override int Id { get; set; } // Primary key
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int FrequencyDays { get; set; }
        public DateTime DueDate { get; set; }
        public String Priority { get; set; }
        [Indexed] public int UserId { get; set; }
        [Indexed] public int CategoryId { get; set; }
        //[Indexed] public int TaskHelpsId { get; set; }

        public override async Task<bool> Add()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = CreatedDate;

            if (_tasksRepository == null)
            {
                throw new InvalidOperationException("TaskRepository is not initialized.");
            }
            return await _tasksRepository.AddTaskAsync(this);
        }

        public override async Task<bool> Get(int id)
        {
            if (_tasksRepository == null)
            {
                throw new InvalidOperationException("TaskRepository is not initialized.");
            }

            var task = await _tasksRepository.GetTaskAsync(id);
            if (task != null)
            {
                // Update the current instance with the retrieved task's properties
                this.Id = task.Id;
                this.CreatedDate = task.CreatedDate;
                this.ModifiedDate = task.ModifiedDate;
                this.Title = task.Title;
                this.Description = task.Description;
                this.Status = task.Status;
                this.FrequencyDays = task.FrequencyDays;
                this.DueDate = task.DueDate;
                this.Priority = task.Priority;
                this.UserId = task.UserId;
                this.CategoryId = task.CategoryId;
                //this.TaskHelpsId = task.TaskHelpsId;
                return true;
            }
            return false;
        }


        public override async Task<bool> Update()
        {
            ModifiedDate = DateTime.Now;
            if (_tasksRepository == null)
            {
                throw new InvalidOperationException("TaskRepository is not initialized.");
            }
            return await _tasksRepository.UpdateTaskAsync(this);
        }





        public override async Task<bool> Delete(int id)
        {
            if (_tasksRepository == null)
            {
                throw new InvalidOperationException("TaskRepository is not initialized.");
            }

            return await _tasksRepository.DeleteTaskAsync(id);
        }

    }
}
