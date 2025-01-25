using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaintainHome.Database;
using MaintainHome.Models;

public class ReportsRepository
{
    private readonly TasksRepository _tasksRepository;
    private readonly TaskActivityRepository _taskActivityRepository; 
    private readonly NotificationRepository _notificationRepository; 
    private readonly PartInfoRepository _partsRepository; 
    private readonly TaskNoteRepository _notesRepository;  
    private readonly TaskHelpRepository _helpRepository;  

    public ReportsRepository(TasksRepository tasksRepository, TaskActivityRepository taskActivityRepository, NotificationRepository notificationRepository, PartInfoRepository partsRepository, TaskNoteRepository notesRepository, TaskHelpRepository helpRepository)
    {
        _tasksRepository = tasksRepository;
        _taskActivityRepository = taskActivityRepository; 
        _notificationRepository = notificationRepository; 
        _partsRepository = partsRepository; 
        _notesRepository = notesRepository; 
        _helpRepository = helpRepository;
    }

    

    public async Task<TaskReport> GenerateTaskScheduleAsync(string period)
    {
        try
        {
            var allTasks = await _tasksRepository.GetTasksAsync();
            var now = DateTime.Today;

            var tasksForPeriod = new List<Tasks>();

            foreach (var task in allTasks)
            {
                if (!task.DueDate.HasValue || task.Status == "Unscheduled")
                {
                    Debug.WriteLine($"Task {task.Title} has a null DueDate or status is Unscheduled: {task.Status}");
                    continue;
                }

                if (task.FrequencyDays > 0)
                {
                    var nextDueDate = task.DueDate.Value;
                    while (nextDueDate <= now.AddYears(1))
                    {
                        if ((period == "Month" && nextDueDate >= now && nextDueDate <= now.AddMonths(1)) ||
                            (period == "3 Months" && nextDueDate >= now && nextDueDate <= now.AddMonths(3)) ||
                            (period == "6 Months" && nextDueDate >= now && nextDueDate <= now.AddMonths(6)) ||
                            (period == "Year" && nextDueDate >= now && nextDueDate <= now.AddYears(1)))
                        {
                            tasksForPeriod.Add(new Tasks
                            {
                                Title = task.Title,
                                DueDate = nextDueDate,
                                Status = task.Status,
                                FrequencyDays = task.FrequencyDays,
                                CreatedDate = task.CreatedDate,
                                ModifiedDate = task.ModifiedDate,
                                CategoryId = task.CategoryId
                            });
                        }
                        nextDueDate = nextDueDate.AddDays(task.FrequencyDays);
                    }
                }
                else
                {
                    if ((period == "Month" && task.DueDate >= now && task.DueDate <= now.AddMonths(1)) ||
                        (period == "3 Months" && task.DueDate > now.AddMonths(1) && task.DueDate <= now.AddMonths(3)) ||
                        (period == "6 Months" && task.DueDate > now.AddMonths(3) && task.DueDate <= now.AddMonths(6)) ||
                        (period == "Year" && task.DueDate > now.AddMonths(6) && task.DueDate <= now.AddYears(1)))
                    {
                        tasksForPeriod.Add(task);
                    }
                }
            }

            // Sort tasks by due date
            tasksForPeriod = tasksForPeriod.OrderBy(t => t.DueDate).ToList();

            var report = new TaskReport
            {
                Period = period,
                DateRange = period == "Month" ? $"({now:MM/dd/yyyy} - {now.AddMonths(1):MM/dd/yyyy})"
                          : period == "3 Months" ? $"({now:MM/dd/yyyy} - {now.AddMonths(3):MM/dd/yyyy})"
                          : period == "6 Months" ? $"({now:MM/dd/yyyy} - {now.AddMonths(6):MM/dd/yyyy})"
                          : $"({now:MM/dd/yyyy} - {now.AddYears(1):MM/dd/yyyy})",
                Tasks = tasksForPeriod
            };

            // Debug logging for the final report
            Debug.WriteLine($"GenerateTaskScheduleAsync Method - Period: {report.Period}, DateRange: {report.DateRange}, Tasks Count: {report.Tasks.Count}");
            foreach (var t in report.Tasks)
            {
                Debug.WriteLine($"   Task: {t.Title}, Due Date: {t.DueDate}");
            }

            return report;
        }
        catch (Exception ex)
        {
            // Log the exception or display an alert to inform the user
            Debug.WriteLine($"An error occurred in GenerateTaskScheduleAsync: {ex.Message}");
            throw new Exception($"An error occurred while generating the task schedule: {ex.Message}");
        }
    }

}

