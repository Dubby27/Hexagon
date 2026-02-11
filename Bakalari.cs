using Hexagon.Screens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        public static UserData? userData;
        public static EventResponse? userEventData;

        //Options
        public static bool BetaQuickTimetable = false;
        public static bool ProcessDetails = false;
        public static bool UpdateTime = true;
        public static int DataSaver = 0;

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
                        try
                        {
                            if (Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel") != null &&
                            Shell.Current.CurrentPage.FindByName<ActivityIndicator>("NetworkActivityIndicator") != null &&
                            Shell.Current.CurrentPage.FindByName<Image>("NetworkBadImage") != null &&
                            Shell.Current.CurrentPage.FindByName<Image>("NetworkGoodImage") != null)
                            {
                                if (ProcessDetails)
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
                                        if (TaskCount == 0)
                                        {
                                            Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text = StatusLabel;
                                        }
                                        else
                                        {
                                            Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text =
                                            StatusLabel + "...";
                                        }
                                    }
                                }
                                else
                                {
                                    if (TaskCount > 0)
                                    {
                                        Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text = "Synchronizace...";
                                    }
                                    else
                                    {
                                        Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text = StatusLabel;
                                    }
                                }

                                Shell.Current.CurrentPage.FindByName<ActivityIndicator>("NetworkActivityIndicator").IsVisible =
                                        StatusActivity;
                                Shell.Current.CurrentPage.FindByName<Image>("NetworkBadImage").IsVisible =
                                    BadImage;
                                Shell.Current.CurrentPage.FindByName<Image>("NetworkGoodImage").IsVisible =
                                    GoodImage;
                                if (BadImage)
                                {
                                    Shell.Current.CurrentPage.FindByName<Image>("NetworkGoodImage").IsVisible =
                                        false;
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            //ignore
                        }
                    };
                    timer.Start();
                    RunningTimers.Add(timer);
                }
            }
        }

        public static bool LastTaskResult = true;

        public static void EndTask(bool result)
        {
            LastTaskResult = result;
            if (TaskCount > 0)
            {
                TaskCount--;
                if(TaskCount == 0)
                {
                    if (result)
                    {
                        StatusActivity = false;
                        GoodImage = true;
                        StatusLabel = "Aktualizováno ";
                        if (UpdateTime)
                        {
                            if (actualValid.Date == DateTime.Today.Date)
                            {
                                StatusLabel += actualValid.ToString("HH:mm");
                            }
                            else
                            {
                                StatusLabel += "před " + (DateTime.Today.Date - actualValid.Date).Days + " dny";
                                Trace.WriteLine(DateTime.Today);
                                Trace.WriteLine(actualValid);
                            }
                        }
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
            else
            {
                if (result)
                {
                    StatusActivity = false;
                    GoodImage = true;
                    StatusLabel = "Aktualizováno ";
                    if (UpdateTime)
                    {
                        if (actualValid.Date == DateTime.Today.Date)
                        {
                            StatusLabel += actualValid.ToString("HH:mm");
                        }
                        else
                        {
                            StatusLabel += "před " + (DateTime.Today.Date - actualValid.Date).Days + " dny";
                            Trace.WriteLine(DateTime.Today);
                            Trace.WriteLine(actualValid);
                        }
                    }
                }
                else
                {
                    StatusActivity = false;
                    BadImage = true;
                    StatusLabel = "Offline";
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

        public class FailResponse
        {
            required public string error { get; set; }
            required public string error_description { get; set; }
        }

        public class ApiVersionResponse()
        {
            required public string ApiVersion { get; set; }
            required public string ApplicationVersion { get; set; }
            required public string BaseUrl { get; set; }
        }

        public static DateTime actualValid;
        public static DateTime nextValid;

        public static async Task<bool> LoadOfflineTimetables()
        {
            if(await SecureStorage.GetAsync("LoggedIn") == "true")
            {
                try
                {
                    actualValid = DateTime.Parse(await SecureStorage.GetAsync("ActualValid"));
                    nextValid = DateTime.Parse(await SecureStorage.GetAsync("NextValid"));
                    if (IsSameIsoWeek(actualValid, DateTime.Today))
                    {
                        actualTimetable = JsonConvert.DeserializeObject<Timetable>(
                            await SecureStorage.GetAsync("ActualTimetable"));
                    }
                    else if (IsSameIsoWeek(nextValid, DateTime.Today))
                    {
                        actualTimetable = JsonConvert.DeserializeObject<Timetable>(
                            await SecureStorage.GetAsync("NextTimetable"));
                    }
                    else
                    {
                        actualTimetable = JsonConvert.DeserializeObject<Timetable>(
                            await SecureStorage.GetAsync("PermanentTimetable"));
                        //Shell.Current.DisplayAlert("Pozor", "Uložený aktuální  rozvrh je příliš zastaralý." +
                        //    " Data budou odvozena ze stálého rozvrhu.", "Rozumím");
                    }

                    if (IsSameIsoWeek(DateTime.Parse(await SecureStorage.GetAsync("NextValid")), DateTime.Today.AddDays(7)))
                    {
                        nextTimetable = JsonConvert.DeserializeObject<Timetable>(
                            await SecureStorage.GetAsync("NextTimetable"));
                    }
                    else
                    {
                        nextTimetable = JsonConvert.DeserializeObject<Timetable>(
                            await SecureStorage.GetAsync("PermanentTimetable"));
                    }
                    permanentTimetable = JsonConvert.DeserializeObject<Timetable>(
                        await SecureStorage.GetAsync("PermanentTimetable"));
                    return true;
                }
                catch
                {
                    Shell.Current.DisplayAlert("Chyba získávání dat", "Tohle by se mělo spravit samo. " +
                        "Pokud problém přetrvává, zkus se odhlásit a přihlásit znovu.", "Ok");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static async Task<bool> LoadOfflineData()
        {
            if (await SecureStorage.GetAsync("LoggedIn") == "true")
            {
                try
                {
                    userData = JsonConvert.DeserializeObject<UserData>(
                        await SecureStorage.GetAsync("UserData"));
                    userEventData = JsonConvert.DeserializeObject<EventResponse>(
                        await SecureStorage.GetAsync("UserEventData"));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        static bool IsSameIsoWeek(DateTime d1, DateTime d2)
        {
            (int year1, int week1) = GetIsoWeekAndYear(d1);
            (int year2, int week2) = GetIsoWeekAndYear(d2);

            return year1 == year2 && week1 == week2;
        }

        static (int isoYear, int isoWeek) GetIsoWeekAndYear(DateTime date)
        {
            // ISO 8601: používá pondělí jako první den týdne a pravidlo "FirstFourDayWeek"
            var cal = CultureInfo.InvariantCulture.Calendar;
            var weekRule = CalendarWeekRule.FirstFourDayWeek;
            var firstDayOfWeek = DayOfWeek.Monday;

            int week = cal.GetWeekOfYear(date, weekRule, firstDayOfWeek);

            // Rok ISO týdne nemusí být stejný jako rok kalendářní
            int year = date.Year;

            // Oprava přelomů: poslední dny prosince mohou patřit do týdne příštího roku
            if (week == 1 && date.Month == 12)
                year++;
            // první dny ledna mohou patřit do posledního týdne minulého roku
            else if (week >= 52 && date.Month == 1)
                year--;

            return (year, week);
        }

        public static List<SchoolListEntry>? schoolList;

        public static async Task<bool> GetSchoolList()
        {
            //log in
            Uri listUrl = new Uri("https://vitskalicky.gitlab.io/bakalari-schools-list/schoolsList.json");
            StartTask("log_in", "Získávání seznamu na " + listUrl);

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(listUrl);
            }
            catch (HttpRequestException ex)
            {
                EndTask(false);
                return false;
            }

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    schoolList = System.Text.Json.JsonSerializer.Deserialize<List<SchoolListEntry>>(responseContent);
                    EndTask(true);
                    return true;
                }
                catch
                {
                    //Error
                    EndTask(false);
                    return false;
                }
            }
            else
            {
                //Error
                EndTask(false);
                return false;
            }
        }

        public static async Task<bool> LogIn(Uri school, string user, string pass)
        {
            //adress check
            Uri checkUri = new Uri(school, "/api");
            StartTask("endpoint_check", "Kontaktování " + checkUri.OriginalString);

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(checkUri);
            }
            catch (HttpRequestException ex)
            {
                EndTask(false);
                OnLogInFinished?.Invoke(false);
                return false;
            }

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
                    try
                    {
                        response = await client.PostAsync(loginUri, content);
                    }
                    catch (HttpRequestException ex)
                    {
                        EndTask(false);
                        OnLogInFinished?.Invoke(false);
                        return false;
                    }

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
                        string r = await response.Content.ReadAsStringAsync();
                        FailResponse? fail = JsonConvert.DeserializeObject<FailResponse>(r);
                        if(fail != null)
                        {
                            if (fail.error_description.Contains("Špatný login nebo heslo."))
                            {
                                Shell.Current.DisplayAlert("Špatné údaje", "Nepodařilo se přihlásit podle zadaných údajů." +
                                " Zkontroluj, jestli máš správně jméno a heslo", "Ok");
                                return false;
                            }
                        }

                        Shell.Current.DisplayAlert("Neznámá chyba", "Server pravděpodobně není dostupný", "Ok");
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

        public static bool refreshInvalid = false;
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

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(checkUri);
            }
            catch(HttpRequestException ex)
            {
                EndTask(false);
                OnLogInFinished?.Invoke(false);
                return false;
            }
            if (response.IsSuccessStatusCode)
            {
                //OK
                EndTask(true);

                //log in
                Uri loginUri = new Uri(school, "/api/login");
                StartTask("log_in", "Přihlašování uživatele pomocí refresh tokenu na " + loginUri.OriginalString);

                HttpContent content = new StringContent("client_id=ANDR&grant_type=refresh_token&refresh_token=" + await SecureStorage.GetAsync("RefreshToken"), Encoding.UTF8, "application/x-www-form-urlencoded");
                try
                {
                    response = await client.PostAsync(loginUri, content);
                }
                catch (HttpRequestException ex)
                {
                    EndTask(false);
                    OnLogInFinished?.Invoke(false);
                    return false;
                }

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
                    string responseContent = await response.Content.ReadAsStringAsync();
                    FailResponse? fail = System.Text.Json.JsonSerializer.Deserialize<FailResponse>(responseContent);

                    if (fail != null)
                    {
                        if (fail.error_description == "The specified refresh token is invalid." ||
                            fail.error_description == "The specified refresh token has already been redeemed.")
                        {
                            if (await SecureStorage.GetAsync("Username") != null &&
                                await SecureStorage.GetAsync("Password") != null)
                            {
                                bool r = await LogIn(school, await SecureStorage.GetAsync("Username"), await SecureStorage.GetAsync("Password"));
                                if (!r)
                                {
                                    await Shell.Current.Navigation.PushAsync(new Hexagon.Screens.LogIn());
                                    await Shell.Current.DisplayAlert("Přihlášení vypršelo", "Musíš zadat své údaje znova." +
                                        " Pokud povolíš uložení údajů, Hexagon bude tvoje přihlášení obnovovat za tebe.\n" +
                                        "Pomocí tlačitka zpět můžeš přeskočit toto přihlášení a zobrazit jen uložená data.", "Ok");
                                    refreshInvalid = true;
                                }
                            }
                            else
                            {
                                await Shell.Current.Navigation.PushAsync(new Hexagon.Screens.LogIn());
                                await Shell.Current.DisplayAlert("Přihlášení vypršelo", "Musíš zadat své údaje znova." +
                                    " Pokud povolíš uložení údajů, Hexagon bude tvoje přihlášení obnovovat za tebe.\n" +
                                    "Pomocí tlačitka zpět můžeš přeskočit toto přihlášení a zobrazit jen uložená data.", "Ok");
                                refreshInvalid = true;
                            }
                        }
                    }

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
            refreshInvalid = false;

            //init refresh
            bool r = await RefreshAll();

            EndTask(r);
            OnLogInFinished?.Invoke(r);
            MainPage.RefreshQuickPanelExt();
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
            if(TaskCount > 0)
            {
                return true;
            }
            UserData? u = await GetUserData();
            if (u == null)
            {
                EndTask(false);
                return false;
            }
            else
            {
                userData = u;
            }
            EventResponse? ue = await GetUserEventData();
            if (u == null)
            {
                EndTask(false);
                return false;
            }
            else
            {
                userEventData = ue;
            }
            bool a = await RefreshActualTimetable();
            if (!a)
            {
                EndTask(false);
                return false;
            }
            bool b = await RefreshNextTimetable();
            if(!b)
            {
                EndTask(false);
                return false;
            }
            bool c = await RefreshPermanentTimetable();
            if(!c)
            {
                EndTask(false);
                return false;
            }

            if (a & b & c)
            {
                MainPage.Instance.RefreshQuickPanel();
                return true;
            }
            else
            {
                EndTask(false);
                return false;
            }
        }

        public static Timetable actualTimetable;
        public static Timetable nextTimetable;
        public static Timetable permanentTimetable;

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
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(uri);
            }
            catch (HttpRequestException ex)
            {
                EndTask(false);
                OnLogInFinished?.Invoke(false);
                return false;
            }

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Timetable? timetableResponse = JsonConvert.DeserializeObject<Timetable>(responseBody);
                if (timetableResponse is not null)
                {
                    actualTimetable = timetableResponse;
                    await SecureStorage.SetAsync("ActualTimetable", responseBody);
                    DateTime validUntil = DateTime.Now;
                    await SecureStorage.SetAsync("ActualValid", validUntil.ToString("O"));
                    actualValid = validUntil;
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

            StartTask("get_next_timetable", "Přenášení dat z " + uri.OriginalString);
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(uri);
            }
            catch (HttpRequestException ex)
            {
                EndTask(false);
                OnLogInFinished?.Invoke(false);
                return false;
            }

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Timetable? timetableResponse = JsonConvert.DeserializeObject<Timetable>(responseBody);
                if (timetableResponse is not null)
                {
                    nextTimetable = timetableResponse;
                    await SecureStorage.SetAsync("NextTimetable", responseBody);
                    DateTime validUntil = DateTime.Now.AddDays(7);
                    await SecureStorage.SetAsync("NextValid", validUntil.ToString("O"));
                    nextValid = validUntil;
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

        public static async Task<bool> RefreshPermanentTimetable()
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

            Uri uri = new(school + "/api/3/timetable/permanent");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentials.access_token);

            StartTask("get_perma_timetable", "Přenášení dat z " + uri.OriginalString);
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(uri);
            }
            catch (HttpRequestException ex)
            {
                EndTask(false);
                OnLogInFinished?.Invoke(false);
                return false;
            }

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Timetable? timetableResponse = JsonConvert.DeserializeObject<Timetable>(responseBody);
                if (timetableResponse is not null)
                {
                    permanentTimetable = timetableResponse;
                    await SecureStorage.SetAsync("PermanentTimetable", responseBody);
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

        //Timetable Getters
        public static TimetableHour? GetTimetableHour(Timetable table, TimetableAtom atom)
        {
            return table.Hours.FirstOrDefault((a) => a.Id == atom.HourId, null);
        }

        public static TimetableSubject? GetTimetableSubject(Timetable table, TimetableAtom atom)
        {
            return table.Subjects.FirstOrDefault((a) => a.Id == atom.SubjectId, null);
        }

        public static TimetableRoom? GetTimetableRoom(Timetable table, TimetableAtom atom)
        {
            return table.Rooms.FirstOrDefault((a) => a.Id == atom.RoomId, null);
        }

        public static TimetableTeacher? GetTimetableTeacher(Timetable table, TimetableAtom atom)
        {
            return table.Teachers.FirstOrDefault((a) => a.Id == atom.TeacherId, null);
        }

        public static List<TimetableGroup>? GetTimetableGroups(Timetable table, TimetableAtom atom)
        {
            List<TimetableGroup> list = new List<TimetableGroup>();
            foreach(string groupId in atom.GroupIds)
            {
                list.Add(table.Groups.FirstOrDefault((a) => a.Id == groupId, null));
            }
            return list;
        }

        //user data
        public static async Task<UserData?> GetUserData()
        {
            if (await ValidateSavedCredentals() == false)
            {
                return null;
            }
            else if (credentials == null)
            {
                await LogInRefresh();
                return null;
            }

            Uri uri = new(school + "/api/3/user");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentials.access_token);

            StartTask("get_user", "Přenášení dat z " + uri.OriginalString);
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(uri);
            }
            catch (HttpRequestException ex)
            {
                EndTask(false);
                OnLogInFinished?.Invoke(false);
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                UserData? dataResponse = JsonConvert.DeserializeObject<UserData>(responseBody);
                if (dataResponse is not null)
                {
                    await SecureStorage.SetAsync("UserData", responseBody);
                    EndTask(true);
                    return dataResponse;
                }
                else
                {
                    EndTask(false);
                    return null;
                }
            }
            else
            {
                EndTask(false);
                return null;
            }

            return null;
        }

        //events
        public static async Task<EventResponse?> GetUserEventData()
        {
            if (await ValidateSavedCredentals() == false)
            {
                return null;
            }
            else if (credentials == null)
            {
                await LogInRefresh();
                return null;
            }

            Uri uri = new(school + "/api/3/events/my?from=" + DateTime.Today.ToString("yyyy-MM-dd"));
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentials.access_token);

            StartTask("get_user_events", "Přenášení dat z " + uri.OriginalString);
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(uri);
            }
            catch (HttpRequestException ex)
            {
                EndTask(false);
                OnLogInFinished?.Invoke(false);
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                EventResponse? dataResponse = JsonConvert.DeserializeObject<EventResponse>(responseBody);
                if (dataResponse is not null)
                {
                    await SecureStorage.SetAsync("UserEventData", responseBody);
                    EndTask(true);
                    return dataResponse;
                }
                else
                {
                    EndTask(false);
                    return null;
                }
            }
            else
            {
                EndTask(false);
                return null;
            }

            return null;
        }
    }

    //User class
    public class UserData
    {
        public string UserUID { get; set; } = "";
        public string FullName { get; set; } = "";
        public string UserType { get; set; } = "";
    }

    //Event class

    public class EventResponse
    {
        public List<Event> events;
    }

    public class Event
    {
        public string Title;
        public string Description;
        public List<EventTime> Times;
    }

    public class EventTime
    {
        public bool WholeDay;
        public string StartTime;
        public string EndTime;
        public EventType EventType;
    }

    public class EventType
    {
        public string Id;
        public string Abbrev;
        public string Name;
    }

    //School list class
    public class SchoolListEntry
    {
        public string name { get; set; } = "";
        public string url { get; set; } = "";
        public string id { get; set; } = "";
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
