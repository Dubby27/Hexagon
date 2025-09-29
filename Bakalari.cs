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

        public static void StartTask(string id, string desc)
        {
            if(TaskCount == 0)
            {
                Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text = desc + "...";
            }
            else
            {
                if (!Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text.Contains("a další úkoly..."))
                {
                    Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text = Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text + " a další úkoly...";
                }
            }
            TaskCount++;
            Shell.Current.CurrentPage.FindByName<ActivityIndicator>("NetworkActivityIndicator").IsVisible = true;
            Shell.Current.CurrentPage.FindByName<Image>("NetworkBadImage").IsVisible = false;
            Shell.Current.CurrentPage.FindByName<Image>("NetworkGoodImage").IsVisible = false;
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
                        Shell.Current.CurrentPage.FindByName<ActivityIndicator>("NetworkActivityIndicator").IsVisible = false;
                        Shell.Current.CurrentPage.FindByName<Image>("NetworkGoodImage").IsVisible = true;
                        Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text = "Aktualizováno";
                    }
                    else
                    {
                        Shell.Current.CurrentPage.FindByName<ActivityIndicator>("NetworkActivityIndicator").IsVisible = false;
                        Shell.Current.CurrentPage.FindByName<Image>("NetworkBadImage").IsVisible = true;
                        Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text = "Offline";
                    }
                }
                else
                {
                    Shell.Current.CurrentPage.FindByName<Label>("NetworkStatusLabel").Text = "Dokončování úkolů...";
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
                            //save credentials
                            await SecureStorage.SetAsync("LoggedIn", "true");
                            await SecureStorage.SetAsync("School", school.ToString());
                            await SecureStorage.SetAsync("RefreshToken", credentials.refresh_token);

                            EndTask(true);
                            OnLogInFinished?.Invoke(true);
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

        //Callbacks
        public static Action<bool> OnLogInFinished;
    }
}
