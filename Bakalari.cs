using Hexagon.Screens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hexagon
{
    static class Bakalari
    {
        public static HttpClient client = new HttpClient();
        public static LoginResponse? credentials;

        //Task Management
        public static int TaskCount = 0;
        public static bool IsSynced = false;
        public static List<IDispatcherTimer> RunningTimers = new List<IDispatcherTimer>();

        public static string StatusLabel = "";
        public static bool StatusActivity = false;
        public static bool BadImage = false;
        public static bool GoodImage = false;

        public static void StartTask(string id, string desc)
        {
            StatusLabel = desc;
            TaskCount++;
            StatusActivity = true;
            BadImage = false;
            GoodImage = false;

            var dispatcher = Application.Current?.Dispatcher;
            bool running = false;
            foreach(IDispatcherTimer timer in RunningTimers)
            {
                running = timer.IsRunning;
                if (!running)
                {
                    RunningTimers.Remove(timer);
                }
            }
            if (running == false)
            {
                IDispatcherTimer? timer = dispatcher?.CreateTimer();
                if (timer != null)
                {
                    timer.Interval = TimeSpan.FromSeconds(0.016);
                    timer.Tick += (s, e) =>
                    {
                        if(Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel") != null &&
                            Shell.Current.CurrentPage.FindByName<ActivityIndicator>("NetworkActivityIndicator") != null &&
                            Shell.Current.CurrentPage.FindByName<Image>("NetworkBadImage") != null &&
                            Shell.Current.CurrentPage.FindByName<Image>("NetworkGoodImage") != null)
                        {
                            if (TaskCount > 1)
                        {
                            if (TaskCount == 2)
                            {
                                Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text =
                                StatusLabel + " a další " + (TaskCount - 1).ToString() + " úkol";
                            }
                            else
                            {
                                Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text =
                                StatusLabel + " a dalších " + (TaskCount - 1).ToString() + " úkolů";
                            }
                        }
                        else
                        {
                            if(TaskCount == 0)
                            {
                                Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text = StatusLabel;
                            }
                            else
                            {
                                Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text =
                                StatusLabel + "...";
                            }
                        }
                        Shell.Current.CurrentPage.FindByName<ActivityIndicator>("NetworkActivityIndicator").IsVisible =
                            StatusActivity;
                        Shell.Current.CurrentPage.FindByName<Image>("NetworkBadImage").IsVisible =
                            BadImage;
                        Shell.Current.CurrentPage.FindByName<Image>("NetworkGoodImage").IsVisible =
                            GoodImage;
                        }
                    };
                    timer.Start();
                    RunningTimers.Add(timer);
                }
            }
        }

        public static void EndTask(bool result)
        {
            if (TaskCount > 0)
            {
                TaskCount--;
                if(TaskCount == 0)
                {
                    if (result)
                    {
                        StatusActivity = false;
                        GoodImage = true;
                        StatusLabel = "Aktualizováno";
                    }
                    else
                    {
                        StatusActivity = false;
                        BadImage = true;
                        StatusLabel = "Offline";
                    }
                }
                else
                {
                    StatusLabel = "Dokončování úkolů";
                }
            }
        }

        //Log In
        public class LoginResponse
        {
            required public string access_token { get; set; }
            required public string token_type { get; set; }
            required public int expires_in { get; set; }
            required public string refresh_token { get; set; }
            required public string scope { get; set; }
        }

        public class ApiVersionResponse()
        {
            required public string ApiVersion { get; set; }
            required public string ApplicationVersion { get; set; }
            required public string BaseUrl { get; set; }
        }

        public static async Task<bool> LogIn(Uri school, string user, string pass)
        {
            //adress check
            Uri checkUri = new Uri(school, "/api");
            StartTask("endpoint_check", "Kontaktování " + checkUri.OriginalString);

            HttpResponseMessage response = await client.GetAsync(checkUri);

            if (response.IsSuccessStatusCode)
            {
                string checkContent = await response.Content.ReadAsStringAsync();
                ApiVersionResponse? check = null;
                try
                {
                    check = JsonConvert.DeserializeObject<List<ApiVersionResponse>>(checkContent)[0];
                }
                catch(Exception ex)
                {
                    //Error
                    EndTask(false);
                    OnLogInFinished?.Invoke(false);
                    Shell.Current.DisplayAlert("Špatná URL školy", "Na dané adrese nebyl nalezen API endpoint Bakalářů." +
                            " Nezapomněl jsi náhodou subdoménu nebo port?", "Ok");
                    return false;
                }

                if(check != null)
                {
                    //OK
                    EndTask(true);

                    //log in
                    Uri loginUri = new Uri(school, "/api/login");
                    StartTask("log_in", "Přihlašování uživatele " + user + " na " + loginUri.OriginalString);

                    HttpContent content = new StringContent("client_id=ANDR&grant_type=password&username=" + user + "&password=" + pass, Encoding.UTF8, "application/x-www-form-urlencoded");
                    response = await client.PostAsync(loginUri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        try
                        {
                            credentials = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(responseContent);
                        }
                        catch
                        {
                            //Error
                            EndTask(false);
                            OnLogInFinished?.Invoke(false);
                            Shell.Current.DisplayAlert("Špatné údaje", "Nepodařilo se přihlásit podle zadaných údajů." +
                                " Zkontroluj, jestli máš správně jméno a heslo", "Ok");
                            return false;
                        }

                        if (credentials != null)
                        {
                            AfterLogIn(school);
                            return true;
                        }
                        else
                        {
                            //Error
                            EndTask(false);
                            Shell.Current.DisplayAlert("Špatné údaje", "Nepodařilo se přihlásit podle zadaných údajů." +
                                " Zkontroluj, jestli máš správně jméno a heslo", "Ok");
                            OnLogInFinished?.Invoke(false);
                            return false;
                        }
                    }
                    else
                    {
                        //Error
                        EndTask(false);
                        OnLogInFinished?.Invoke(false);
                        Shell.Current.DisplayAlert("Špatné údaje", "Nepodařilo se přihlásit podle zadaných údajů." +
                                " Zkontroluj, jestli máš správně jméno a heslo", "Ok");
                        return false;
                    }
                }
                else
                {
                    //Error
                    EndTask(false);
                    Shell.Current.DisplayAlert("Špatná URL školy", "Na dané adrese nebyl nalezen API endpoint Bakalářů." +
                        " Nezapomněl jsi náhodou subdoménu nebo port?", "Ok");
                    OnLogInFinished?.Invoke(false);
                    return false;
                }
            }
            else
            {
                //Error
                EndTask(false);
                OnLogInFinished?.Invoke(false);
                Shell.Current.DisplayAlert("Špatná URL školy", "Na dané adrese nebyl nalezen API endpoint Bakalářů." +
                    " Nezapomněl jsi náhodou subdoménu nebo port?", "Ok");
                return false;
            }
        }

        public static async Task<bool> LogInRefresh()
        {
            if (await ValidateSavedCredentals() == false)
            {
                return false;
            }

            Uri school = new Uri(await SecureStorage.GetAsync("School"));

            //adress check
            Uri checkUri = new(school, relativeUri:"/api");
            StartTask("endpoint_check", "Kontaktování " + checkUri.OriginalString);

            HttpResponseMessage response = await client.GetAsync(checkUri);

            if (response.IsSuccessStatusCode)
            {
                //OK
                EndTask(true);

                //log in
                Uri loginUri = new Uri(school, "/api/login");
                StartTask("log_in", "Přihlašování uživatele pomocí refresh tokenu na " + loginUri.OriginalString);

                HttpContent content = new StringContent("client_id=ANDR&grant_type=refresh_token&refresh_token=" + await SecureStorage.GetAsync("RefreshToken"), Encoding.UTF8, "application/x-www-form-urlencoded");
                response = await client.PostAsync(loginUri, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    credentials = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(responseContent);

                    if (credentials != null)
                    {
                        AfterLogIn(school);
                        return true;
                    }
                    else
                    {
                        //Error
                        EndTask(false);
                        OnLogInFinished?.Invoke(false);
                        return false;
                    }
                }
                else
                {
                    //Error
                    EndTask(false);
                    OnLogInFinished?.Invoke(false);
                    return false;
                }
            }
            else
            {
                //Error
                EndTask(false);
                OnLogInFinished?.Invoke(false);
                return false;
            }
        }

        public static async void AfterLogIn(Uri school)
        {
            //save credentials
            await SecureStorage.SetAsync("LoggedIn", "true");
            await SecureStorage.SetAsync("School", school.ToString());
            Bakalari.school = school;
            await SecureStorage.SetAsync("RefreshToken", credentials.refresh_token);

            //init refresh
            await RefreshAll();

            EndTask(true);
            OnLogInFinished?.Invoke(true);
        }

        public async static Task<bool> ValidateSavedCredentals()
        {
            if (await SecureStorage.GetAsync("School") is not string school)
            {
                await Shell.Current.DisplayAlert("Chyba Připojení", "Nepodařilo se přihlásit pomocí uložených údajů. Prosím, přihlašte se znovu.", "OK");
                await Shell.Current.Navigation.PushModalAsync(new LogIn(), false);
                return false;
            }
            else if (string.IsNullOrEmpty(school))
            {
                await Shell.Current.DisplayAlert("Chyba Připojení", "Nepodařilo se přihlásit pomocí uložených údajů. Prosím, přihlašte se znovu.", "OK");
                await Shell.Current.Navigation.PushModalAsync(new LogIn(), false);
                return false;
            }
            else if(await SecureStorage.GetAsync("RefreshToken") is not string token)
            {
                await Shell.Current.DisplayAlert("Chyba Připojení", "Nepodařilo se přihlásit pomocí uložených údajů. Prosím, přihlašte se znovu.", "OK");
                await Shell.Current.Navigation.PushModalAsync(new LogIn(), false);
                return false;
            }
            else if (string.IsNullOrEmpty(token))
            {
                await Shell.Current.DisplayAlert("Chyba Připojení", "Nepodařilo se přihlásit pomocí uložených údajů. Prosím, přihlašte se znovu.", "OK");
                await Shell.Current.Navigation.PushModalAsync(new LogIn(), false);
                return false;
            }
            Bakalari.school = new Uri(await SecureStorage.GetAsync("School"));
            return true;
        }

        //Refreshing data
        public static Uri school;
        public static async Task<bool> RefreshAll()
        {
            await RefreshActualTimetable();
            await RefreshNextTimetable();
            return true;
        }

        public static Timetable actualTimetable;
        public static Timetable nextTimetable;

        //Refreshing timetables
        public static async Task<bool> RefreshActualTimetable()
        {
            if(await ValidateSavedCredentals() == false)
            {
                return false;
            }
            else if(credentials == null)
            {
                await LogInRefresh();
                return false;
            }

            Uri uri = new(school + "/api/3/timetable/actual");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentials.access_token);

            StartTask("get_actual_timetable", "Přenášení dat z " + uri.OriginalString);
            HttpResponseMessage response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Timetable? timetableResponse = JsonConvert.DeserializeObject<Timetable>(responseBody);
                if (timetableResponse is not null)
                {
                    actualTimetable = timetableResponse;
                    await SecureStorage.SetAsync("ActualTimetable", responseBody);
                    EndTask(true);
                }
                else
                {
                    EndTask(false);
                    return false;
                }
            }
            else
            {
                EndTask(false);
                return false;
            }

            return true;
        }

        public static async Task<bool> RefreshNextTimetable()
        {
            if (await ValidateSavedCredentals() == false)
            {
                return false;
            }
            else if (credentials == null)
            {
                await LogInRefresh();
                return false;
            }

            Uri uri = new(school + "/api/3/timetable/actual?date=" + DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"));
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentials.access_token);

            StartTask("get_actual_timetable", "Přenášení dat z " + uri.OriginalString);
            HttpResponseMessage response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Timetable? timetableResponse = JsonConvert.DeserializeObject<Timetable>(responseBody);
                if (timetableResponse is not null)
                {
                    nextTimetable = timetableResponse;
                    await SecureStorage.SetAsync("NextTimetable", responseBody);
                    EndTask(true);
                }
                else
                {
                    EndTask(false);
                    return false;
                }
            }
            else
            {
                EndTask(false);
                return false;
            }

            return true;
        }

        //Callbacks
        public static Action<bool> OnLogInFinished;
    }

    //Timetable classes
    public class TimetableHour
    {
        public int Id { get; set; }
        public string Caption { get; set; } = "";
        public string BeginTime { get; set; } = "";
        public string EndTime { get; set; } = "";
    }

    public class TimetableChange
    {
        public string ChangeSubject { get; set; } = "";
        public string Day { get; set; } = "";
        public string Hours { get; set; } = "";
        public string ChangeType { get; set; } = "";
        public string Description { get; set; } = "";
        public string TypeAbbrev { get; set; } = "";
        public string TypeName { get; set; } = "";
    }

    public class TimetableAtom
    {
        public int HourId { get; set; }
        public List<string> GroupIds { get; set; } = new List<string>();
        public string SubjectId { get; set; } = "";
        public string TeacherId { get; set; } = "";
        public string RoomId { get; set; } = "";
        public bool IsLastRoomLesson { get; set; } = false;
        public List<string> CycleIds { get; set; } = new List<string>();
        public TimetableChange? Change { get; set; } = null;
        public List<string> HomeworkIds { get; set; } = new List<string>();
        public string? Theme { get; set; } = null;
    }

    public class TimetableClass
    {
        public string Id { get; set; } = "";
        public string Abbrev { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class TimetableDay
    {
        public List<TimetableAtom> Atoms { get; set; } = new List<TimetableAtom>();
        public int DayOfWeek { get; set; }
        public string Date { get; set; } = "";
        public string DayType { get; set; } = "";
    }

    public class TimetableGroup
    {
        public string ClassId { get; set; } = "";
        public string Id { get; set; } = "";
        public string Abbrev { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class TimetableSubject
    {
        public string Id { get; set; } = "";
        public string Abbrev { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class TimetableTeacher
    {
        public string Id { get; set; } = "";
        public string Abbrev { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class TimetableRoom
    {
        public string Id { get; set; } = "";
        public string Abbrev { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class TimetableCycle
    {
        public string Id { get; set; } = "";
        public string Abbrev { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class Timetable
    {
        public List<TimetableHour> Hours { get; set; } = new List<TimetableHour>();
        public List<TimetableDay> Days { get; set; } = new List<TimetableDay>();
        public List<TimetableClass> Classes { get; set; } = new List<TimetableClass>();
        public List<TimetableGroup> Groups { get; set; } = new List<TimetableGroup>();
        public List<TimetableSubject> Subjects { get; set; } = new List<TimetableSubject>();
        public List<TimetableTeacher> Teachers { get; set; } = new List<TimetableTeacher>();
        public List<TimetableRoom> Rooms { get; set; } = new List<TimetableRoom>();
        public List<TimetableCycle> Cycles { get; set; } = new List<TimetableCycle>();
    }

}
