using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OrderMeetingRoom
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<Record> Records = new ObservableCollection<Record>();
        ObservableCollection<string> Status = new ObservableCollection<string>();
        ObservableCollection<User> Users = new ObservableCollection<User>();
        ObservableCollection<string> DaysOfWeek = new ObservableCollection<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};
        public MainPage()
        {
            this.InitializeComponent();
            DateTimeOffset dateTime = DateTimeOffset.Now;
            StartDatePicker.Date = dateTime.Date;
            StartTimePicker.Time = new TimeSpan(dateTime.Hour,dateTime.Minute,0);
            EndDatePicker.Date = (dateTime+TimeSpan.FromHours(1)).Date;
            EndTimePicker.Time = new TimeSpan((dateTime + TimeSpan.FromHours(1)).Hour, dateTime.Minute, 0);
            RoomComboBox.SelectedIndex = 6;
            RecordListView.ItemsSource = Records;
            StatusListView.ItemsSource = Status;
            LoadUsers();
            UserComboBox.ItemsSource = Users;
            DayOfWeekComboBox.ItemsSource = DaysOfWeek;
            DayOfWeekComboBox.SelectedItem = dateTime.DayOfWeek.ToString();
        }

        public async void LoadUsers()
        {
            StorageFile file =await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("Users.json", CreationCollisionOption.OpenIfExists);

            string userstxt = await FileIO.ReadTextAsync(file);
            if (JsonArray.TryParse(userstxt, out JsonArray UsersJsonArray))
            {
                for (uint i = 0; i < UsersJsonArray.Count; i++)
                {
                    User user = new User();
                    user.UserId = (int)UsersJsonArray.GetObjectAt(i).GetNamedNumber("uid");
                    user.UserName = UsersJsonArray.GetObjectAt(i).GetNamedString("uname");
                    if (!Users.Contains(user))
                    {
                        Users.Add(user);
                    }
                }
            }

            User u = new User { UserId = 88, UserName = "刘刚" };
            if (!Users.Contains(u))
            {
                Users.Add(u);
            }
            UserComboBox.SelectedItem = u;
            u = new User { UserId = 89, UserName = "张英杰" };
            if (!Users.Contains(u))
            {
                Users.Add(u);
            }
            u = new User { UserId = 128, UserName = "魏子豪" };
            if (!Users.Contains(u))
            {
                Users.Add(u);
            }
        }

        private async void QueryRoomButton_Click(object sender, RoutedEventArgs e)
        {
            DateTimeOffset StartDateTime = (DateTimeOffset)StartDatePicker.Date.Value.Date + StartTimePicker.Time;
            DateTimeOffset EndDateTime = (DateTimeOffset)EndDatePicker.Date.Value.Date + EndTimePicker.Time;

            var filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
            HttpClient httpClient = new HttpClient(filter); HttpStringContent content = new HttpStringContent("st="+StartDateTime.ToString("yyyy-MM-dd HH:mm:ss")+"&et="+EndDateTime.ToString("yyyy-MM-dd HH:mm:ss"), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");

            HttpRequestResult requestResult = await httpClient.TryPostAsync(new Uri("https://a.rouor.com/LoisMeeting/record/room"), content);
            if(requestResult.Succeeded)
            {
                if(JsonObject.TryParse(requestResult.ResponseMessage.Content.ToString(),out JsonObject resultJsonObject))
                {
                    if (resultJsonObject.GetNamedNumber("r") == 0)
                    {
                        JsonArray dataJsonArray = resultJsonObject.GetNamedArray("d");
                        if(dataJsonArray.Count<=0)
                        {
                            Status.Insert(0, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ")+"該時間段内沒有可以會議室，請重新選擇時段！");
                            return;
                        }
                        else
                        {
                            string room = StartDateTime.ToString("yyyy/MM/dd HH:mm--")+EndDateTime.ToString("yyyy/MM/dd HH:mm") + "空閑會議室有：";
                            for(uint i=0;i<dataJsonArray.Count;i++)
                            {
                                room += dataJsonArray.GetObjectAt(i).GetNamedString("name") + "; ";
                            }
                            Status.Insert(0, room);
                        }
                    }
                    else
                    {
                        Status.Insert(0, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ") + resultJsonObject.GetNamedString("e"));
                    }
                }
            }
        }

        private async void OrderRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if(StartTimePicker.Time.Minutes%15!=0)
            {
                StartTimePicker.Time = StartTimePicker.Time - TimeSpan.FromMinutes(StartTimePicker.Time.Minutes % 15);
            }
            if (EndTimePicker.Time.Minutes % 15 != 0)
            {
                EndTimePicker.Time = EndTimePicker.Time + TimeSpan.FromMinutes(15-EndTimePicker.Time.Minutes % 15);
            }
            DateTimeOffset StartDateTime = (DateTimeOffset)StartDatePicker.Date.Value.Date + StartTimePicker.Time;
            DateTimeOffset EndDateTime = (DateTimeOffset)EndDatePicker.Date.Value.Date + EndTimePicker.Time;

            if(RepeatBox.IsChecked==false)
            {
                EndDateTime = (DateTimeOffset)StartDatePicker.Date.Value.Date + EndTimePicker.Time;
            }
            string dayofweek = DayOfWeekComboBox.SelectedItem.ToString();
            int rid = RoomComboBox.SelectedIndex + 1;
            User user = UserComboBox.SelectedItem as User;
            int uid = user.UserId;
            var filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
            for (DateTimeOffset datetime = StartDateTime; datetime <= EndDateTime; datetime = datetime + TimeSpan.FromDays(1))
            {
                if(datetime.DayOfWeek.ToString()!=dayofweek)
                {
                    continue;
                }
                HttpClient httpClient = new HttpClient(filter);
                HttpStringContent content = new HttpStringContent("rid=" + rid + "&uid="+uid+"&st=" + datetime.ToString("yyyy-MM-dd HH:mm:ss") + "&et=" + datetime.ToString("yyyy-MM-dd ")+ EndDateTime.ToString("HH:mm:ss") + "&desc=討論班", Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");

                HttpRequestResult requestResult = await httpClient.TryPostAsync(new Uri("https://a.rouor.com/LoisMeeting/record/add"), content);
                if (requestResult.Succeeded)
                {
                    if (JsonObject.TryParse(requestResult.ResponseMessage.Content.ToString(), out JsonObject resultJsonObject))
                    {
                        if (resultJsonObject.GetNamedNumber("r") == 0)
                        {
                            Status.Insert(0, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ") + "預訂成功");
                        }
                        else
                        {
                            Status.Insert(0, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ") + resultJsonObject.GetNamedString("e"));
                        }
                    }
                }
            }
        }

        private async void QueryRecordButton_Click(object sender, RoutedEventArgs e)
        {
            Records.Clear();
            DateTimeOffset StartDateTime = (DateTimeOffset)StartDatePicker.Date.Value.Date + StartTimePicker.Time;
            DateTimeOffset EndDateTime = (DateTimeOffset)EndDatePicker.Date.Value.Date + EndTimePicker.Time;
            User user = UserComboBox.SelectedItem as User;

            var filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
            HttpClient httpClient = new HttpClient(filter);

            HttpStringContent content = new HttpStringContent("uid="+user.UserId+"&st=" + StartDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "&et=" + EndDateTime.ToString("yyyy-MM-dd HH:mm:ss")+"&pi=0&ps=100", Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");

            HttpRequestResult requestResult = await httpClient.TryPostAsync(new Uri("https://a.rouor.com/LoisMeeting/record/my"), content);
            if (requestResult.Succeeded)
            {
                if (JsonObject.TryParse(requestResult.ResponseMessage.Content.ToString(), out JsonObject resultJsonObject))
                {
                    if (resultJsonObject.GetNamedNumber("r") == 0)
                    {
                        JsonArray dataJsonArray = resultJsonObject.GetNamedArray("d");
                        if (dataJsonArray.Count <= 0)
                        {
                            Status.Insert(0, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ") + "該時間段内沒有預訂會議室！");
                            return;
                        }
                        else
                        {
                            for (uint i = 0; i < dataJsonArray.Count; i++)
                            {
                                Record record = new Record();
                                record.Id= (int)dataJsonArray.GetObjectAt(i).GetNamedNumber("id");
                                if (!Records.Contains(record))
                                {
                                    record.StartDateTime = DateTimeOffset.Parse(dataJsonArray.GetObjectAt(i).GetNamedString("st"));
                                    record.EndDateTime = DateTimeOffset.Parse(dataJsonArray.GetObjectAt(i).GetNamedString("et"));
                                    record.MeetingRoom = dataJsonArray.GetObjectAt(i).GetNamedString("rname");
                                    record.UserName = dataJsonArray.GetObjectAt(i).GetNamedString("uname");
                                    record.Description = dataJsonArray.GetObjectAt(i).GetNamedString("desc");
                                    Records.Insert((int)i,record);
                                }
                            }
                        }
                    }
                    else
                    {
                        Status.Insert(0, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ") + resultJsonObject.GetNamedString("e"));
                    }
                }
            }
        }

        private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //DateTimeOffset StartDateTime = (DateTimeOffset)StartDatePicker.Date.Value.Date + StartTimePicker.Time;
            //DateTimeOffset EndDateTime = (DateTimeOffset)EndDatePicker.Date.Value.Date + EndTimePicker.Time;
            Button button = sender as Button;
            Record record = button.DataContext as Record;

            var filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
            HttpClient httpClient = new HttpClient(filter); HttpStringContent content = new HttpStringContent("id=" + record.Id, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");

            HttpRequestResult requestResult = await httpClient.TryPostAsync(new Uri("https://a.rouor.com/LoisMeeting/record/del"), content);
            if (requestResult.Succeeded)
            {
                if (JsonObject.TryParse(requestResult.ResponseMessage.Content.ToString(), out JsonObject resultJsonObject))
                {
                    if (resultJsonObject.GetNamedNumber("r") == 0)
                    {
                        //Record record = new Record();
                        //record.StartDateTime = StartDateTime;
                        //record.EndDateTime = EndDateTime;
                        //record.MeetingRoom = RoomComboBox.SelectedItem.ToString();
                        //record.UserName = "劉剛";
                        //record.Description = "討論班";
                        //Records.Insert(0, record);
                        Records.Remove(record);
                        Status.Insert(0, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ") + "取消成功");
                    }
                    else
                    {
                        Status.Insert(0, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ") + resultJsonObject.GetNamedString("e"));
                    }
                }
            }
        }

        private void StartDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            DateTimeOffset StartDateTime = (DateTimeOffset)StartDatePicker.Date.Value.Date + StartTimePicker.Time;
            if (EndDatePicker.Date != null && EndTimePicker.Time != null)
            {
                DateTimeOffset EndDateTime = (DateTimeOffset)EndDatePicker.Date.Value.Date + EndTimePicker.Time;
                if (EndDateTime < StartDateTime)
                {
                    EndDateTime = StartDateTime + TimeSpan.FromHours(1);
                    EndDatePicker.Date = (EndDateTime + TimeSpan.FromHours(1)).Date;
                    EndTimePicker.Time = new TimeSpan((EndDateTime + TimeSpan.FromHours(1)).Hour, EndDateTime.Minute, 0);
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if(checkBox.IsChecked==true)
            {
                DayOfWeekComboBox.Visibility = Visibility.Visible;
                EndDatePicker.Visibility = Visibility.Visible;

            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked == false)
            {
                DayOfWeekComboBox.Visibility = Visibility.Collapsed;
                EndDatePicker.Visibility = Visibility.Collapsed;

            }
        }
    }
}
