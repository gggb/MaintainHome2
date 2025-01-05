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

    public async Task<List<TaskReport>> GenerateTaskReportAsync()
    {
        try
        {
            var allTasks = await _tasksRepository.GetTasksAsync();
            var now = DateTime.Today;

            var monthTasks = new List<Tasks>();
            var threeMonthTasks = new List<Tasks>();
            var sixMonthTasks = new List<Tasks>();
            var yearTasks = new List<Tasks>();

            foreach (var task in allTasks)
            {
                // Check for null DueDate and handle accordingly
                if (!task.DueDate.HasValue) 
                { 
                    Debug.WriteLine($"Task {task.Title} has a null DueDate"); 
                    continue; 
                }
                if (task.FrequencyDays > 0)
                {
                    var nextDueDate = task.DueDate.Value;
                    while (nextDueDate <= now.AddYears(1))
                    {
                        if (nextDueDate >= now && nextDueDate <= now.AddMonths(1))
                        {
                            monthTasks.Add(task);
                        }
                        else if (nextDueDate > now.AddMonths(1) && nextDueDate <= now.AddMonths(3))
                        {
                            threeMonthTasks.Add(task);
                        }
                        else if (nextDueDate > now.AddMonths(3) && nextDueDate <= now.AddMonths(6))
                        {
                            sixMonthTasks.Add(task);
                        }
                        else if (nextDueDate > now.AddMonths(6) && nextDueDate <= now.AddYears(1))
                        {
                            yearTasks.Add(task);
                        }

                        nextDueDate = nextDueDate.AddDays(task.FrequencyDays);
                    }
                }
                else
                {
                    if (task.DueDate >= now && task.DueDate <= now.AddMonths(1))
                    {
                        monthTasks.Add(task);
                    }
                    else if (task.DueDate > now.AddMonths(1) && task.DueDate <= now.AddMonths(3))
                    {
                        threeMonthTasks.Add(task);
                    }
                    else if (task.DueDate > now.AddMonths(3) && task.DueDate <= now.AddMonths(6))
                    {
                        sixMonthTasks.Add(task);
                    }
                    else if (task.DueDate > now.AddMonths(6) && task.DueDate <= now.AddYears(1))
                    {
                        yearTasks.Add(task);
                    }
                }
            }

            var report = new List<TaskReport>
            {
                new TaskReport { Period = "Month", DateRange = $"({now:MM/dd/yyyy} - {now.AddMonths(1):MM/dd/yyyy})", Tasks = monthTasks.Distinct().ToList() },
                new TaskReport { Period = "3 Months", DateRange = $"({now:MM/dd/yyyy} - {now.AddMonths(3):MM/dd/yyyy})", Tasks = threeMonthTasks.Distinct().ToList() },
                new TaskReport { Period = "6 Months", DateRange = $"({now:MM/dd/yyyy} - {now.AddMonths(6):MM/dd/yyyy})", Tasks = sixMonthTasks.Distinct().ToList() },
                new TaskReport { Period = "Year", DateRange = $"({now:MM/dd/yyyy} - {now.AddYears(1):MM/dd/yyyy})", Tasks = yearTasks.Distinct().ToList() }
            };

            // Debug logging for the final reports
            Debug.WriteLine("GenerateTaskReportAsync Method:"); 
            foreach (var r in report) 
            { 
                Debug.WriteLine($"*****Period: {r.Period}, Tasks Count: {r.Tasks.Count}"); 
                foreach (var t in r.Tasks) 
                { 
                    Debug.WriteLine($"   Task: {t.Title}, Due Date: {t.DueDate}"); 
                } 
            }

            return report;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Report.Repository Error generating task report: {ex.Message}");
            return new List<TaskReport>();
        }
    }

    public async Task<List<TaskReport>> GenerateTaskScheduleAsync8()
    {
        var allTasks = await _tasksRepository.GetTasksAsync();
        var now = DateTime.Today;

        var monthTasks = new List<Tasks>();
        var threeMonthTasks = new List<Tasks>();
        var sixMonthTasks = new List<Tasks>();
        var yearTasks = new List<Tasks>();

        foreach (var task in allTasks)
        {
            // Check for null DueDate and handle accordingly
            if (!task.DueDate.HasValue)
            {
                Debug.WriteLine($"Task {task.Title} has a null DueDate");
                continue;
            }

            if (task.FrequencyDays > 0)
            {
                var nextDueDate = task.DueDate.Value;
                while (nextDueDate <= now.AddYears(1))
                {
                    if (nextDueDate >= now && nextDueDate <= now.AddMonths(1))
                    {
                        monthTasks.Add(new Tasks
                        {
                            Title = task.Title,
                            DueDate = nextDueDate,
                            Status = task.Status,
                            FrequencyDays = task.FrequencyDays
                        });
                    }
                    else if (nextDueDate > now.AddMonths(1) && nextDueDate <= now.AddMonths(3))
                    {
                        threeMonthTasks.Add(new Tasks
                        {
                            Title = task.Title,
                            DueDate = nextDueDate,
                            Status = task.Status,
                            FrequencyDays = task.FrequencyDays
                        });
                    }
                    else if (nextDueDate > now.AddMonths(3) && nextDueDate <= now.AddMonths(6))
                    {
                        sixMonthTasks.Add(new Tasks
                        {
                            Title = task.Title,
                            DueDate = nextDueDate,
                            Status = task.Status,
                            FrequencyDays = task.FrequencyDays
                        });
                    }
                    else if (nextDueDate > now.AddMonths(6) && nextDueDate <= now.AddYears(1))
                    {
                        yearTasks.Add(new Tasks
                        {
                            Title = task.Title,
                            DueDate = nextDueDate,
                            Status = task.Status,
                            FrequencyDays = task.FrequencyDays
                        });
                    }

                    nextDueDate = nextDueDate.AddDays(task.FrequencyDays);
                }
            }
            else
            {
                if (task.DueDate >= now && task.DueDate <= now.AddMonths(1))
                {
                    monthTasks.Add(task);
                }
                else if (task.DueDate > now.AddMonths(1) && task.DueDate <= now.AddMonths(3))
                {
                    threeMonthTasks.Add(task);
                }
                else if (task.DueDate > now.AddMonths(3) && task.DueDate <= now.AddMonths(6))
                {
                    sixMonthTasks.Add(task);
                }
                else if (task.DueDate > now.AddMonths(6) && task.DueDate <= now.AddYears(1))
                {
                    yearTasks.Add(task);
                }
            }
        }

        //Sort tasks by due date
        monthTasks = monthTasks.OrderBy(t => t.DueDate).ToList();
        threeMonthTasks = threeMonthTasks.OrderBy(t => t.DueDate).ToList(); 
        sixMonthTasks = sixMonthTasks.OrderBy(t => t.DueDate).ToList(); 
        yearTasks = yearTasks.OrderBy(t => t.DueDate).ToList();

        var report = new List<TaskReport>
        {
            new TaskReport { Period = "Month", DateRange = $"({now:MM/dd/yyyy} - {now.AddMonths(1):MM/dd/yyyy})", Tasks = monthTasks },
            new TaskReport { Period = "3 Months", DateRange = $"({now:MM/dd/yyyy} - {now.AddMonths(3):MM/dd/yyyy})", Tasks = threeMonthTasks },
            new TaskReport { Period = "6 Months", DateRange = $"({now:MM/dd/yyyy} - {now.AddMonths(6):MM/dd/yyyy})", Tasks = sixMonthTasks },
            new TaskReport { Period = "Year", DateRange = $"({now:MM/dd/yyyy} - {now.AddYears(1):MM/dd/yyyy})", Tasks = yearTasks }
        };

        // Debug logging for the final reports
        Debug.WriteLine("GenerateTaskReportAsync Method:");
        foreach (var r in report)
        {
            Debug.WriteLine($"*****Period: {r.Period}, Tasks Count: {r.Tasks.Count}");
            foreach (var t in r.Tasks)
            {
                Debug.WriteLine($"   Task: {t.Title}, Due Date: {t.DueDate}");
            }
        }
        return report;
    }

    public async Task<TaskReport> GenerateTaskScheduleAsync(string period)
    {
        var allTasks = await _tasksRepository.GetTasksAsync();
        var now = DateTime.Today;

        var tasksForPeriod = new List<Tasks>();

        foreach (var task in allTasks)
        {
            if (!task.DueDate.HasValue)
            {
                Debug.WriteLine($"Task {task.Title} has a null DueDate");
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
                            FrequencyDays = task.FrequencyDays
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




    public async Task<List<TaskReport>> GenerateDetailedTaskReportAsync()
    {
        var allTasks = await _tasksRepository.GetTasksAsync();

        foreach (var task in allTasks)
        {
            var taskActivities = await _taskActivityRepository.GetTaskActivitiesAsyncByTaskId(task.Id);
            var notifications = await _notificationRepository.GetNotificationsAsyncByTaskId(task.Id);
            var parts = await _partsRepository.GetAllPartsInfoAsyncByTaskId(task.Id);
            var notes = await _notesRepository.GetAllTaskNotesAsyncByTaskId(task.Id);
            var helps = await _helpRepository.GetAllTaskHelpsAsyncByTaskId(task.Id);

            var detailedTaskReport = new DetailedTaskReport 
            { 
                Task = task, 
                Activities = taskActivities, 
                Notifications = notifications, 
                Parts = parts, 
                Notes = notes, 
                Helps = helps 
            };
        }

        var now = DateTime.Today;
        var report = new List<TaskReport>
    {
        new TaskReport
        {
            Title = "Detailed Task Report",
            Period = "All",
            DateRange = $"({now:MM/dd/yyyy})",
            Tasks = allTasks
        }
    };

        return report;
    }

    public async Task<List<ComprehensiveTaskReport>> GenerateComprehensiveTaskReportAsync()
    {
        var allTasks = await _tasksRepository.GetTasksAsync();
        var comprehensiveReports = new List<ComprehensiveTaskReport>();

        foreach (var task in allTasks)
        {
            var taskActivities = await _taskActivityRepository.GetTaskActivitiesAsyncByTaskId(task.Id);
            var notifications = await _notificationRepository.GetNotificationsAsyncByTaskId(task.Id);
            var parts = await _partsRepository.GetAllPartsInfoAsyncByTaskId(task.Id);
            var notes = await _notesRepository.GetAllTaskNotesAsyncByTaskId(task.Id);
            var helps = await _helpRepository.GetAllTaskHelpsAsyncByTaskId(task.Id);

            var comprehensiveReport = new ComprehensiveTaskReport
            {
                TaskTitle = task.Title,
                TaskDueDate = task.DueDate ?? DateTime.MinValue,  // Handling nullable DateTime
                TaskStatus = task.Status,
                Period = "All",
                DateRange = $"{DateTime.Today:MM/dd/yyyy}",
                Activities = taskActivities,
                Notifications = notifications,
                Parts = parts,
                Notes = notes,
                Helps = helps,
                DateTimeStamp = DateTime.Now
            };

            comprehensiveReports.Add(comprehensiveReport);
        }

        return comprehensiveReports;
    }


}

