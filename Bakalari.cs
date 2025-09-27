using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Hexagon
{
    static class Bakalari
    {
        public static HttpClient client = new HttpClient();
        public static LoginResponse credentials;

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

        public static async Task<bool> LogIn(Uri school, string user, string pass)
        {
            //adress check
            Uri checkUri = new Uri(school, "/api");
            StartTask("endpoint_check", "Kontaktování " + checkUri.OriginalString);

            HttpResponseMessage response = await client.GetAsync(checkUri);

            if (response.IsSuccessStatusCode)
            {
                //OK
                EndTask(true);

                //log in
                Uri loginUri = new Uri(school, "/api/login");
                StartTask("log_in", "Přihlašování uživatele " + user + " na " + loginUri.OriginalString);

                HttpContent content = new StringContent("client_id=ANDR&grant_type=password&username=" + user + "&password=" + pass, Encoding.UTF8, "application/x-www-form-urlencoded");
                response = await client.PostAsync(loginUri, content);

                if(response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    credentials = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(responseContent);

                    if(credentials != null)
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

        //Callbacks
        public static Action<bool> OnLogInFinished;
    }
}
