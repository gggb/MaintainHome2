using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MaintainHome.Database;
using MaintainHome.Models;
using SQLite;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MaintainHome.Views
{

    public partial class Reports : ContentPage
    {
        public ObservableCollection<TaskReport> TaskReports { get; set; }
        private readonly ReportsRepository _reportsRepository;
        private readonly TasksRepository tasksRepository;

        private readonly TaskActivityRepository taskActivityRepository;
        private readonly NotificationRepository notificationRepository;
        private readonly PartInfoRepository partInfoRepository;
        private readonly TaskNoteRepository taskNoteRepository;
        private readonly TaskHelpRepository taskHelpRepository;

        public Reports()
        {
            InitializeComponent();
            TaskReports = new ObservableCollection<TaskReport>();
            TaskDumpCollectionView.ItemsSource = TaskReports;

            try
            {
                // Initialize each repository
                tasksRepository = new TasksRepository();
                taskActivityRepository = new TaskActivityRepository();
                notificationRepository = new NotificationRepository();
                partInfoRepository = new PartInfoRepository();
                taskNoteRepository = new TaskNoteRepository();
                taskHelpRepository = new TaskHelpRepository();

                // Initialize ReportsRepository with all repositories
                _reportsRepository = new ReportsRepository(
                    tasksRepository,
                    taskActivityRepository,
                    notificationRepository,
                    partInfoRepository,
                    taskNoteRepository,
                    taskHelpRepository
                );
                Console.WriteLine("ReportsRepository initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ReportsRepository: {ex.Message}");
            }
        }

        private async void OnMonthClicked(object sender, EventArgs e)
        {
            await LoadReports("Month");
        }
        private async void OnThreeMonthsClicked(object sender, EventArgs e)
        {
            await LoadReports("3 Months");
        }
        private async void OnSixMonthsClicked(object sender, EventArgs e)
        {
            await LoadReports("6 Months");
        }
        private async void OnYearClicked(object sender, EventArgs e)
        {
            await LoadReports("Year");
        }
        private async Task LoadReports(string period)
        {
            try
            {
                if (_reportsRepository == null)
                {
                    Debug.WriteLine("ReportRepository is null!");
                    return;
                }

                // Clear the current list
                TaskReports.Clear();

                TaskScheduleView.IsVisible = true;
                TaskDumpView.IsVisible = false;
                ReportName.Text = $"Task Schedule Report - {period}";
                ReportDate.Text = $"Report generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                // Fetch task reports from the repository
                var report = await _reportsRepository.GenerateTaskScheduleAsync(period);

                // Set the date range
                ReportDateRange.Text = report.DateRange;

                // Add the main report
                //TaskReports.Add(report);
                Debug.WriteLine($"Added main report for period {period} with title {report.Title}");

                // Add task details
                foreach (var task in report.Tasks)
                {
                    var taskReport = new TaskReport
                    {
                        Title   = task.Title,
                        IsBold  =  false,
                        DueDate = task.DueDate ?? DateTime.MinValue,
                        Status  =  task.Status,
                        FrequencyDays = task.FrequencyDays,
                        CategoryId = task.CategoryId,
                        CreatedDate = task.CreatedDate,
                        ModifiedDate = task.ModifiedDate
                    };
                    TaskReports.Add(taskReport);
                    Debug.WriteLine($"Added task report: {taskReport.Title} with due date {taskReport.DueDate}");
                }

                // Refresh the collection view to ensure changes are displayed
                TaskScheduleCollectionView.ItemsSource = null;
                TaskScheduleCollectionView.ItemsSource = TaskReports;
                Debug.WriteLine($"ReportsCollectionView updated with {TaskReports.Count} items");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading reports for period {period}: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                TaskReports.Add(new TaskReport
                {
                    Title = $"Error loading reports for period {period}: {ex.Message}",
                    IsBold = false
                });

                // Refresh the collection view to ensure changes are displayed
                TaskScheduleCollectionView.ItemsSource = null;
                TaskScheduleCollectionView.ItemsSource = TaskReports;
            }
        }
        private async void OnTaskDumpClicked8(object sender, EventArgs e)
        {
            try
            {
                // Set the DateTimeStamp for the header
                //HeaderDateTime.Text = $"Report created on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                // Fetch all data into lists
                var tasksList = await tasksRepository.GetTasksAsync();

                // Clear the current list
                TaskReports.Clear();

                foreach (var task in tasksList)
                {
                    // Add the task details first
                    TaskReports.Add(new TaskReport
                    {
                        Title = $"Task: {task.Title}",
                        DueDate = task.DueDate ?? DateTime.MinValue,
                        Status = task.Status,
                        IsBold = true
                    });

                    // Fetch and print related activities
                    try
                    {
                        var relatedActivities = await taskActivityRepository.GetTaskActivitiesAsyncByTaskId(task.Id);
                        if (relatedActivities.Any())
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Activities:"
                            });
                            foreach (var activity in relatedActivities)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {activity.Action}",
                                    IsBold = false
                                });
                            }                           
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   No Activities",
                            });
                        }
                        //TaskReports.Add(new TaskReport
                        //{
                        //    Title = "  "
                        //});
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching activities for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Activities: Error fetching activities"
                        });
                    }

                    // Fetch and print related parts
                    try
                    {
                        var relatedParts = await partInfoRepository.GetAllPartsInfoAsyncByTaskId(task.Id);
                        if (relatedParts.Any())
                        { 
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Parts:"
                            });                            
                            foreach (var part in relatedParts)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {part.Name}",
                                    IsBold = false
                                });
                            }
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "    No Parts"
                            });
                        }
                        //TaskReports.Add(new TaskReport
                        //{
                        //    Title = "  "
                        //});
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching parts for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Parts: Error fetching parts"
                        });
                    }

                    // Fetch and print related notifications
                    try
                    {
                        var relatedNotifications = await notificationRepository.GetNotificationsAsyncByTaskId(task.Id);
                        if (relatedNotifications.Any())
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Notifications:"
                            });
                            foreach (var notification in relatedNotifications)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {notification.Message}",
                                    IsBold = false
                                });
                            }
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   No Notifications"
                            });
                        }
                        //TaskReports.Add(new TaskReport
                        //{
                        //    Title = "  "
                        //});
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching notifications for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Notifications: Error fetching notifications"
                        });
                    }

                    // Fetch and print related notes
                    try
                    {
                        var relatedNotes = await taskNoteRepository.GetAllTaskNotesAsyncByTaskId(task.Id);
                        if (relatedNotes.Any())
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Notes:"
                            });
                            foreach (var note in relatedNotes)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {note.NoteContent}",
                                    IsBold = false
                                });
                            }
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   No Notes"
                            });
                        }
                        //TaskReports.Add(new TaskReport
                        //{
                        //    Title = "  "
                        //});
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching notes for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Notes: Error fetching notes"
                        });
                    }

                    // Fetch and print related helps
                    try
                    {
                        var relatedHelps = await taskHelpRepository.GetAllTaskHelpsAsyncByTaskId(task.Id);
                        if (relatedHelps.Any())
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Helps:"
                            });
                            foreach (var help in relatedHelps)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {help.Description}",
                                    IsBold = false
                                });
                            }
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   No Helps"
                            });
                        }
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  "
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching helps for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Helps: Error fetching helps"
                        });
                    }
                }

                // Refresh the collection view to ensure changes are displayed
                TaskDumpCollectionView.ItemsSource = null;
                TaskDumpCollectionView.ItemsSource = TaskReports;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching tasks: {ex.Message}");
                TaskReports.Add(new TaskReport
                {
                    Title = "Error fetching tasks",
                    IsBold = false
                });

                // Refresh the collection view to ensure changes are displayed
                TaskDumpCollectionView.ItemsSource = null;
                TaskDumpCollectionView.ItemsSource = TaskReports;
            }
        }

        private async void OnTaskDumpClicked(object sender, EventArgs e)
        {
            try
            {
                // Set the views and headers for Task Dump Report
                TaskScheduleView.IsVisible = false;
                TaskDumpView.IsVisible = true;
                ReportName.Text = "Task Dump Report";
                ReportDate.Text = $"Report generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                ReportDateRange.Text = string.Empty;

                // Clear the current list
                TaskReports.Clear();

                // Fetch all data into lists
                var tasksList = await tasksRepository.GetTasksAsync();

                foreach (var task in tasksList)
                {
                    // Add the task details first
                    TaskReports.Add(new TaskReport
                    {
                        Title = $"Task: {task.Title}",
                        DueDate = task.DueDate ?? DateTime.MinValue,
                        Status = task.Status,
                        IsBold = true
                    });

                    // Fetch and print related activities
                    try
                    {
                        var relatedActivities = await taskActivityRepository.GetTaskActivitiesAsyncByTaskId(task.Id);
                        if (relatedActivities.Any())
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Activities:"
                            });
                            foreach (var activity in relatedActivities)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {activity.Action}",
                                    IsBold = false
                                });
                            }
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   No Activities"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching activities for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Activities: Error fetching activities"
                        });
                    }

                    // Fetch and print related parts
                    try
                    {
                        var relatedParts = await partInfoRepository.GetAllPartsInfoAsyncByTaskId(task.Id);
                        if (relatedParts.Any())
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Parts:"
                            });
                            foreach (var part in relatedParts)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {part.Name}",
                                    IsBold = false
                                });
                            }
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   No Parts"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching parts for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Parts: Error fetching parts"
                        });
                    }

                    // Fetch and print related notifications
                    try
                    {
                        var relatedNotifications = await notificationRepository.GetNotificationsAsyncByTaskId(task.Id);
                        if (relatedNotifications.Any())
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Notifications:"
                            });
                            foreach (var notification in relatedNotifications)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {notification.Message}",
                                    IsBold = false
                                });
                            }
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   No Notifications"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching notifications for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Notifications: Error fetching notifications"
                        });
                    }

                    // Fetch and print related notes
                    try
                    {
                        var relatedNotes = await taskNoteRepository.GetAllTaskNotesAsyncByTaskId(task.Id);
                        if (relatedNotes.Any())
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Notes:"
                            });
                            foreach (var note in relatedNotes)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {note.NoteContent}",
                                    IsBold = false
                                });
                            }
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   No Notes"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching notes for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Notes: Error fetching notes"
                        });
                    }

                    // Fetch and print related helps
                    try
                    {
                        var relatedHelps = await taskHelpRepository.GetAllTaskHelpsAsyncByTaskId(task.Id);
                        if (relatedHelps.Any())
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   Helps:"
                            });
                            foreach (var help in relatedHelps)
                            {
                                TaskReports.Add(new TaskReport
                                {
                                    Title = $"     - {help.Description}",
                                    IsBold = false
                                });
                            }
                        }
                        else
                        {
                            TaskReports.Add(new TaskReport
                            {
                                Title = "   No Helps"
                            });
                        }
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  "
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching helps for task {task.Id}: {ex.Message}");
                        TaskReports.Add(new TaskReport
                        {
                            Title = "  Helps: Error fetching helps"
                        });
                    }
                }

                // Refresh the collection view to ensure changes are displayed
                TaskDumpCollectionView.ItemsSource = null;
                TaskDumpCollectionView.ItemsSource = TaskReports;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching tasks: {ex.Message}");
                TaskReports.Add(new TaskReport
                {
                    Title = "Error fetching tasks",
                    IsBold = false
                });

                // Refresh the collection view to ensure changes are displayed
                TaskDumpCollectionView.ItemsSource = null;
                TaskDumpCollectionView.ItemsSource = TaskReports;
            }
        }




    }









}
