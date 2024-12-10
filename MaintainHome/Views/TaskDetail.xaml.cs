using MaintainHome.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MaintainHome.Database;
using Plugin.LocalNotification;
//using Windows.Networking.PushNotifications;
//using MediaToolbox;

namespace MaintainHome.Views
{   
    public partial class TaskDetail : ContentPage
    {
        private Tasks _task;

        public ObservableCollection<string> StatusOptions { get; set; } =
            new ObservableCollection<string> { "Unscheduled", "Scheduled", "Completed", "Canceled" };

        public ObservableCollection<string> PriorityOptions { get; set; } =
            new ObservableCollection<string> { "Low", "Medium", "Urgent" };
      
        public TaskDetail(Tasks task)
        {
            InitializeComponent();
            _task = task ?? new Tasks(); // Initialize the task

            // Initialize options
            StatusOptions = new ObservableCollection<string>
            {
                "Unscheduled", "Scheduled", "Completed", "Canceled"
            };
                PriorityOptions = new ObservableCollection<string>
            {
                "Low", "Medium", "Urgent"
            };

            // Assign BindingContext to the page instance
            BindingContext = _task;
            Debug.WriteLine("*********TaskDetail Page Initialized");
            Debug.WriteLine("*********StatusOptions: " + string.Join(", ", StatusOptions));
            Debug.WriteLine("*********PriorityOptions: " + string.Join(", ", PriorityOptions));

            try
            {
                bool taskZero = _task.Id == 0;
                if (taskZero)
                {
                    Debug.WriteLine("Creating new task form");
                    _task.DueDate = DateTime.Today;
                    DueDateDatePicker.Date = DateTime.Today;
                }
                else
                {
                    Debug.WriteLine("Loading existing task details");
                    DueDateDatePicker.Date = _task.DueDate ?? DateTime.Today;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing TaskDetail: {ex.Message}");

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while initializing the task details.", "OK");
                });
            }
        }    
        

        public void OnSaveButtonClicked(object sender, EventArgs e)
        {
            // Call the async method and handle it properly
            OnSaveButtonClickedAsync(sender, e).ConfigureAwait(false);
        }

        public async Task OnSaveButtonClickedAsync(object sender, EventArgs e)
        {
            //Ensure _task is not null
            if (_task == null)
            {
                return;
            }

            // Validate TaskDetail edit entries.
            var task = (Tasks)BindingContext;
            var validationResults = ValidateTask(task);

            // Save edited TaskDetail data, if validation successful
            if (validationResults.Count == 0)
            {
                // Update Tasks table  
                var repository = new TasksRepository();
                bool goodUpdate = await repository.UpdateTaskAsync(task);
                if (goodUpdate)
                {
                    await DisplayAlert("Confirm Update", "The Task record has been successfully updated.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to update the task.", "OK"); return; // Exit if the update fails
                }


                // Ensure notification permissions are granted. It is perfered to ensure permission is granted before
                // every send (although permission was requested during app startup).                   
                bool isNotifyPermissionsGranted = await LocalNotificationCenter.Current.AreNotificationsEnabled();
                if (!isNotifyPermissionsGranted)
                {
                    await LocalNotificationCenter.Current.RequestNotificationPermission();
                }

                //Cancel existing notification for this task (Both, the PlugInNotification table record and the notification sent to Android) 
                var notificationRepository = new PlugInNotificationRepository();
                var existingNotification = await notificationRepository.GetPlugInNotificationByTaskIdAsync(task.Id);
                Debug.WriteLine($"**********PluginNotification Removal Section*****: {existingNotification}");
                if (existingNotification != null)
                {
                    Debug.WriteLine($"**********Previous notification found: {existingNotification.NotificationId}");
                    bool wasNotificationCanceled = LocalNotificationCenter.Current.Cancel(existingNotification.NotificationId);
                    Debug.WriteLine($"**********Was notification canceled: {wasNotificationCanceled}");
                    if (!wasNotificationCanceled)
                    {
                        Debug.WriteLine($"*********Failed to cancel notification with ID: {existingNotification.NotificationId}");
                    }

                    bool dbNotificationDeleteded = await notificationRepository.DeletePlugInNotificationAsync(existingNotification.NotificationId);
                    Debug.WriteLine($"*************Was notification removed from Plugin table: {dbNotificationDeleteded}");
                }
                Debug.WriteLine($"**********PluginNotification Removal Ended********************");

                // Define NotifyTime based on debug or release mode
                DateTime notifyTime;
                DateTime preNotifyTime;

#if DEBUG
                preNotifyTime = DateTime.Now.AddSeconds(10);
                notifyTime = DateTime.Now.AddSeconds(20);

#else
                if (task.DueDate.HasValue)
                {
                    task.DueDate = new DateTime(task.DueDate.Value.Year, task.DueDate.Value.Month, task.DueDate.Value.Day, 9, 0, 0);
                    notifyTime = task.DueDate ?? DateTime.Now;
                    preNotifyTime = task.DueDate.Value.AddDays(-3);
                }
                else
                {
                    notifyTime = DateTime.Now; 
                    preNotifyTime = DateTime.Now;
                }
#endif


                // Handle notifications (anew) based on task status
                if (task.Status == "Scheduled")
                {
                    // Schedule new Pre-alert notification based on DueDate - 3 days
                    var notifyRequestPreAlert = new NotificationRequest     //Prepare message to be sent.
                    {
                        NotificationId = task.Id,
                        Title = task.Title,
                        Description = $"A task, {task.Title}, is scheduled in three days.",
                        Schedule = new NotificationRequestSchedule { NotifyTime = preNotifyTime },
                    };
                    bool wasNotifyRequestSuccessful = await LocalNotificationCenter.Current.Show(notifyRequestPreAlert);      //Send Reminder Message-- scheduled for DueDate  
                    Debug.WriteLine($"************Did pre-alert request execute successfully: {wasNotifyRequestSuccessful} Notice scheduled to display: {preNotifyTime}");


                    // Update description and time for the due date reminder
                    var notifyRequest = new NotificationRequest     //Prepare message to be sent.
                    {
                        NotificationId = task.Id,
                        Title = task.Title,
                        Description = $"A task, {task.Title}, is scheduled on {task.DueDate}.",
                        Schedule = new NotificationRequestSchedule { NotifyTime = notifyTime },
                    };
                    wasNotifyRequestSuccessful = await LocalNotificationCenter.Current.Show(notifyRequest);      //Send pre-alert Message-- scheduled for DueDate -3 days
                    Debug.WriteLine($"*************Did notifyrequest execute successfully: {wasNotifyRequestSuccessful}  Notice scheduled to display: {notifyTime}");

                    // Update PluginNotification table
                    Debug.WriteLine($"**********PluginNotification Update Section********************");
                    var newNotification = new PlugInNotification
                    {
                        TaskId = task.Id,
                        //NotificationId = notifyRequest.NotificationId, Notification id is the auto-incremented primary key.
                        NotificationDate = notifyRequest.Schedule.NotifyTime ?? DateTime.Now,
                        NotificationType = "Reminder",
                        NotificationTitle = notifyRequest.Title,
                        NotificationDescription = notifyRequest.Description,
                        NotificationIsSent = true
                    };
                    await notificationRepository.AddOrUpdatePlugInNotificationAsync(newNotification);

                    Debug.WriteLine($"**********PluginNotification Update section End***** " +
                        $"ID:{newNotification.NotificationId}, " +
                        $"TaskId: {newNotification.TaskId}, " +
                        $"DueDate: {newNotification.NotificationDate}, " +
                        $"Type: {newNotification.NotificationType}," +
                        $"Title: {newNotification.NotificationTitle}, " +
                        $"Descr: {newNotification.NotificationDescription}, " +
                        $"IsSent: {newNotification.NotificationIsSent}");

                    Debug.WriteLine($"Scheduled notification for {task.Title} on {task.DueDate}");
                }
                else if (task.Status == "Completed")
                {
                    // Optionally, send completion notification immediately
                    var notifyRequest = new NotificationRequest
                    {
                        NotificationId = task.Id,
                        Title = $"Task {task.Title} Completed",
                        Description = $"The task {task.Title} has been marked as completed.",
                        Schedule = new NotificationRequestSchedule { NotifyTime = notifyTime }
                    };

                    await LocalNotificationCenter.Current.Show(notifyRequest);
                    Debug.WriteLine($"Sent completion notification for {task.Title}");

                    // Update PluginNotification table
                    await notificationRepository.AddOrUpdatePlugInNotificationAsync(new PlugInNotification
                    {
                        TaskId = task.Id,
                        NotificationId = notifyRequest.NotificationId,
                        NotificationDate = notifyRequest.Schedule.NotifyTime ?? DateTime.Now,
                        NotificationType = "Completion",
                        NotificationTitle = notifyRequest.Title,
                        NotificationDescription = notifyRequest.Description,
                        NotificationIsSent = true
                    });
                }
                else if (task.Status == "Canceled")
                {
                    // Optionally, send cancellation notification immediately
                    var notifyRequest = new NotificationRequest
                    {
                        NotificationId = task.Id,
                        Title = $"Task {task.Title} Canceled",
                        Description = $"The task {task.Title} has been canceled.",
                        Schedule = new NotificationRequestSchedule { NotifyTime = DateTime.Now }
                    };

                    await LocalNotificationCenter.Current.Show(notifyRequest);
                    Debug.WriteLine($"Sent cancellation notification for {task.Title}");

                    // Update PluginNotification table
                    await notificationRepository.AddOrUpdatePlugInNotificationAsync(new PlugInNotification
                    {
                        TaskId = task.Id,
                        NotificationId = notifyRequest.NotificationId,
                        NotificationDate = notifyRequest.Schedule.NotifyTime ?? DateTime.Now,
                        NotificationType = "Canceled",
                        NotificationTitle = notifyRequest.Title,
                        NotificationDescription = notifyRequest.Description,
                        NotificationIsSent = true
                    });
                }
            }
            else
            {
                // Display validation errors
                var errorMessage = string.Join(Environment.NewLine, validationResults);
                await DisplayAlert("Validation Error", errorMessage, "OK");
            }
        }
        

        private List<string> ValidateTask(Tasks task)
        {
            var validationResults = new List<string>();

            if (string.IsNullOrWhiteSpace(task.Title))
            {
                validationResults.Add("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(task.Description))
            {
                validationResults.Add("Description is required.");
            }

            if (task.CategoryId <= 0)
            {
                validationResults.Add("Category is required.");
            }

            if (!task.DueDate.HasValue) 
            { 
                validationResults.Add("Due date is required."); 
            } 
            else if (task.DueDate.Value <= DateTime.Now.AddSeconds(-59)) 
            { 
                validationResults.Add("Due date must be in the future (greater than the current date/time minus 59 seconds)."); 
            }

            if (task.FrequencyDays <= 0)
            {
                validationResults.Add("Category is required.");
            }

            if (string.IsNullOrWhiteSpace(task.Priority))
            {
                validationResults.Add("Priority is required.");
            }

            if (string.IsNullOrWhiteSpace(task.Status))
            {
                validationResults.Add("Status is required.");
            }



            return validationResults;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("TaskDetail Page Appearing");

            try
            {
                // Load TaskActivityControl with the taskId
                await TaskActivityControl.LoadData(_task.Id);

                // Load NotificationControl with the taskId
                await NotificationControl.LoadData(_task.Id);

                // Load NoteControl with the taskId
                await NoteControl.LoadData(_task.Id);

                // Load NoteControl with the taskId
                await HelpControl.LoadData(_task.Id);

                // Load NoteControl with the taskId
                await PartControl.LoadData(_task.Id);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while loading task activities or notifications.", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("TaskDetail Page Disappearing");

            try
            {
                // Any additional cleanup code can go here
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnDisappearing: {ex.Message}");
                // No need for a DisplayAlert here, since this is less critical
            }
        }
    }
}
    

