using MaintainHome.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

using MaintainHome.Database;
using Plugin.LocalNotification;
//using EventKit;

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
        }    //apparently a click button event cannot call an async, non-void method, so i created a 
        public async Task OnSaveButtonClickedAsync(object sender, EventArgs e)
        {           
            //Ensure _task is not null
            if (_task == null)
            {
                return;
            }

            // Save confirmation message
            bool answer = await DisplayAlert("Confirm Action", "Do you want to schedule this task?", "Yes", "No");
            if (!answer)
            {
                return; // If "No" is selected, return and do not proceed further
            }


            var task = (Tasks)BindingContext;
            // Ensure Duedate is greater than now + 65 minutes for the notification to work correctly.
            var today = DateTime.Today;
            task.DueDate = DateTime.Now.AddMinutes(65); // Ensure DueDate is set to now + 65 minutes for testing and notifications
            //if(task.DueDate.HasValue && task.DueDate.Value.Date == today)
            //{ 
            //    task.DueDate = DateTime.Now.AddMinutes(65); 
            //}

            // Validate TaskDetail edit entries.
            var validationResults = ValidateTask(task);

            // Save edited TaskDetail data, if validation successful
            if (validationResults.Count == 0)
            {
                // Initialize repository for add or updating tasks      // Perhaps, this task update should be moved to the end to 
                var repository = new TasksRepository();                 // prevent dbl save confirmation alerts.
                bool goodUpdate;
                if (task.Id == 0)   // task is new
                {
                    task.UserId = App.CurrentUser.UserId;
                    goodUpdate = await repository.AddTaskAsync(task);
                }
                else
                {
                    // Update existing task
                    goodUpdate = await repository.UpdateTaskAsync(task);
                }
                
                if (!goodUpdate)
                {
                    await DisplayAlert("Error", "Failed to update the task. Notification not sent.", "OK"); 
                    return; // Exit if the update fails WITHOUT SENDING NOTIFICATION.
                }
                
                // Ensure notification permissions are granted. It is perfered to ensure permission is granted before
                // every send (although permission was requested during app startup).                   
                bool isNotifyPermissionsGranted = await LocalNotificationCenter.Current.AreNotificationsEnabled();
                if (!isNotifyPermissionsGranted)
                {
                    await LocalNotificationCenter.Current.RequestNotificationPermission();
                }

                // Cancel existing task notification and delete associated record from the PlugInNotification table.

                // NOTE: Only one notification record should be in the PlugInNotification table (because only one "on due-date" notification is sent) but the 'get'
                // operation retrieves multiple records and puts them in a list so as to validate the logic. There should NEVER be more than one validation record
                // in the database table per task.Id.
                var notificationRepository = new PlugInNotificationRepository();
                var existingNotifications = await notificationRepository.GetPlugInNotificationsByTaskIdAsync(task.Id);
                Debug.WriteLine($"**********PluginNotification Removal Section*****: {existingNotifications}");
                //int _count = 0;
                if (existingNotifications != null && existingNotifications.Count > 0)
                {
                    foreach (var notification in existingNotifications)  //TaskHelpRepository should only be one!
                    {
                        Debug.WriteLine($"**********Previous {existingNotifications.Count} notification(s) found: {notification.NotificationId}");
                        bool wasNotificationCanceled = LocalNotificationCenter.Current.Cancel(notification.NotificationId);

                        Debug.WriteLine($"**********Was notification ({notification.NotificationId}) canceled from Plugin: {wasNotificationCanceled}");
                        if (!wasNotificationCanceled)
                        {
                            Debug.WriteLine($"*********Failed to cancel notification with ID: {notification.NotificationId}");
                        }

                        bool dbNotificationDeleteded = await notificationRepository.DeletePlugInNotificationAsync(notification.NotificationId);
                        Debug.WriteLine($"**************Was notification ({notification.NotificationId}) canceled from database: {dbNotificationDeleteded}");
                    }
                }
                Debug.WriteLine($"**********PluginNotification Removal Ended********************");

                // Define NotifyTime based on debug or release mode
                DateTime notifyTime;


#if DEBUG
                notifyTime = DateTime.Now.AddSeconds(10);   // for testing purposes, the tester can't wait until DueDate to test the notification, so it is sent in 10 seconds.
#else
                // Release mode
                if (task.DueDate.HasValue)    // In production, notification will be sent immediately but will display in Android on DueDate at current time + 60 minutes..
                {
                    notifyTime = task.DueDate.Value;
                }
                else   // in case task.DueDate has no value, set it to current time (this should NEVER happen)!!!
                {
                    await DisplayAlert("DueDate Alert", "DueDate has no value and is set to the currentdate/time plus 60 minutes.", "OK");
                    notifyTime = DateTime.Now.AddMinutes(60).AddSeconds(60); // Ensure it meets the 59 minutes requirement
                }
#endif

                // Handle notifications (anew) based on task status. 
                if (task.Status == "Scheduled")
                {
                    await ScheduleNotification(task, notifyTime, goodUpdate);
                }
                else if (task.Status == "Completed")
                {
                    // Completed tasks should Always notify immediately.
                    // await SendCompletionNotification(task, notifyTime);   
                    await SendCompletionNotification(task, DateTime.Now.AddSeconds(10));

                    // Check for reoccurring task and reset status Due Date for notification.
                    if (task.FrequencyDays > 0)
                    {
                        task.Status = "Scheduled";
                        if (task.DueDate.HasValue) 
                        { 
                            task.DueDate = task.DueDate.Value.AddDays(task.FrequencyDays); 
                        }
                        else
                        { // Handle the case where DueDate is null
                            task.DueDate = DateTime.Now.AddDays(task.FrequencyDays); 
                        }
#if DEBUG  
                        notifyTime = DateTime.Now.AddSeconds(10);   // for testng purposes, the tester can't wait until DueDate to test the notification, so it is sent in 20 seconds.
#else  
                        notifyTime = (task.DueDate ?? DateTime.Now).AddMinutes(60); // In release mode, ensures notifyTime is at least 60 minutes in the future.
#endif

                        await ScheduleNotification(task, notifyTime, goodUpdate);
                        bool goodTaskReset = await repository.UpdateTaskAsync(task);
                        if (goodTaskReset)
                        {
                            DueDateDatePicker.Date = task.DueDate ?? DateTime.Now;
                            await DisplayAlert("Task Reset Confirm", $"The task, '{task.Title}', has been completed and reset to start on {task.DueDate}.", "OK");
                            BindingContext = null; BindingContext = task;
                        }
                        else
                        {
                            await DisplayAlert("Task Reset ERROR", $"The task, {task.Title}, could not be reset.", "OK");
                        }
                    }
                }
                else if (task.Status == "Canceled")
                {
                    await SendCancellationNotification(task);
                }
            }           
            else
            {
                // Display validation errors
                var errorMessage = string.Join(Environment.NewLine, validationResults);
                await DisplayAlert("Validation Error", errorMessage, "OK");
            }
        }
        public void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            // Call the async method and handle it properly
            OnDeleteButtonClickedAsync(sender, e).ConfigureAwait(false);
        }
        public async Task OnDeleteButtonClickedAsync(object sender, EventArgs e)
        {
            if (_task == null)
            {
                return;
            }

            // Display a confirmation message
            bool confirmDelete = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this task and all associated data?", "Yes", "No");
            if (!confirmDelete)
            {
                return; // Exit if the user does not confirm the deletion
            }

            
            // Delete related data
            await DeleteRelatedDataAsync(_task.Id);


            // Delete Task
            var repository = new TasksRepository();        // Initialize task repository
            bool isDeleted = await repository.DeleteTaskAsync(_task.Id);

            if (isDeleted)
            {
                // Call SendCancellationNotification method to cancel the notification
                await SendCancellationNotification(_task);

                await DisplayAlert("Success", "Task successfully deleted.", "OK");
                await Navigation.PopAsync();

                // delete task notification
                
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete the task.", "OK");
            }
        }       
        private async Task DeleteRelatedDataAsync(int taskId)
        {
            var partsRepository = new PartInfoRepository();
            var notificationsRepository = new NotificationRepository();
            var activitiesRepository = new TaskActivityRepository();
            var helpsRepository = new TaskHelpRepository();
            var notesRepository = new TaskNoteRepository();
            var plugInNotificationRepository = new PlugInNotificationRepository();

            // Delete related parts
            await partsRepository.DeletePartsByTaskIdAsync(taskId);

            // Delete related notifications
            await notificationsRepository.DeleteNotificationsByTaskIdAsync(taskId);

            // Delete related activities
            await activitiesRepository.DeleteActivitiesByTaskIdAsync(taskId);

            // Delete related helps
            await helpsRepository.DeleteHelpsByTaskIdAsync(taskId);

            // Delete related notes
            await notesRepository.DeleteNotesByTaskIdAsync(taskId);

            // Delete related PlugInNotification
            var plugInNotifications = await plugInNotificationRepository.GetPlugInNotificationsByTaskIdAsync(taskId); 
            foreach (var plugInNotification in plugInNotifications) 
            { 
                await plugInNotificationRepository.DeletePlugInNotificationAsync(plugInNotification.NotificationId); 
            }



        }
        private async Task ScheduleNotification(Tasks task, DateTime notifyTime, bool goodUpdate)
        {
            Debug.WriteLine($"***Schedule Notification method STARTING********************");
            Debug.WriteLine($"************* preparing notification data*******************");
            var notificationRepository = new PlugInNotificationRepository();

            // Schedule new notification based on DueDate
            var notifyRequest = new NotificationRequest
            {
                NotificationId = task.Id,
                Title = task.Title,
                Description = $"A task, {task.Title}, is scheduled on {notifyTime}.",
                Schedule = new NotificationRequestSchedule 
                { 
                  NotifyTime = notifyTime,
                    RepeatType = NotificationRepeat.TimeInterval,
#if DEBUG
                    NotifyRepeatInterval = TimeSpan.FromSeconds(10)
#else
                    NotifyRepeatInterval = TimeSpan.FromDays(7)
#endif
                },
            };

            // Display alert with notification data
            await DisplayAlert("Notification Data",
                $"Title: {notifyRequest.Title}\n" +
                $"Description: {notifyRequest.Description}\n" +
                $"NotifyTime: {notifyRequest.Schedule.NotifyTime}\n" +
                $"Repeat Interval: {notifyRequest.Schedule.NotifyRepeatInterval}", "OK");

            Debug.WriteLine($"************Sending notification to the 3rd party plugin*******************");
            bool wasNotifyRequestSuccessful = await LocalNotificationCenter.Current.Show(notifyRequest);
            Debug.WriteLine($"************(1) Notification 'Show' request sent successfully?: {wasNotifyRequestSuccessful} Due-date: {notifyTime}");

            // Update PluginNotification table
            var notification = new PlugInNotification
            {
                TaskId = task.Id,
                NotificationId = task.Id,
                NotificationDate = notifyTime,
                NotificationType = "DueDate Reminder",
                NotificationTitle = notifyRequest.Title,
                NotificationDescription = notifyRequest.Description,
                NotificationIsSent = true
            };
           Debug.WriteLine($"************* Prepared notification record (Notification-id & Notification date): {notification.NotificationId} & {notifyTime}");

            Debug.WriteLine($"***********PluginNotification Update data: " +
                $"NotificationID:{notification.NotificationId}," +
                $"TaskId: {notification.TaskId}," +
                $"DueDate: {notification.NotificationDate}," +
                $"Type: {notification.NotificationType}," +
                $"Title: {notification.NotificationTitle}, " +
                $"Descr: {notification.NotificationDescription}, " +
                $"IsSent: {notification.NotificationIsSent}");

            await notificationRepository.AddOrUpdatePlugInNotificationAsync(notification);
            Debug.WriteLine($"***********Uodated Scheduled notification for {task.Title} on {notification.NotificationDate}");

            Debug.WriteLine($"***Schedule Notification Method ENDING********************");

        }
        private async Task SendCompletionNotification(Tasks task, DateTime notifyTime)
        {
            var notificationRepository = new PlugInNotificationRepository();

            // Send completion notification immediately
            var notifyRequest = new NotificationRequest
            {
                NotificationId = task.Id,
                Title = $"Task {task.Title} Completed",
                Description = $"The task {task.Title} has been marked as completed.",
                Schedule = new NotificationRequestSchedule { NotifyTime = notifyTime }
            };

            Debug.WriteLine($"Sending ****COMPLETION notification**** for {task.Title}"); 
            await LocalNotificationCenter.Current.Show(notifyRequest);
            Debug.WriteLine($"Sent ****COMPLETION notification**** for {task.Title} at {DateTime.Now}");

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
        private async Task SendCancellationNotification(Tasks task)
        {
            var notificationRepository = new PlugInNotificationRepository();

            // Send cancellation notification immediately
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
    

