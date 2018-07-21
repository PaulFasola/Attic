using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using lang = Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Popups;

namespace Infoécran.Classes
{
    public static class Utilities
    {
        public static string RemoveAccents(this string libelle)
        {
            if (string.IsNullOrEmpty(libelle)) return libelle;
            char[] oldChar = { 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'à', 'á', 'â', 'ã', 'ä', 'å', 'Ò', 'Ó', 'Ô', 'Õ', 'Ö', 'Ø', 'ò', 'ó', 'ô', 'õ', 'ö', 'ø', 'È', 'É', 'Ê', 'Ë', 'è', 'é', 'ê', 'ë', 'Ì', 'Í', 'Î', 'Ï', 'ì', 'í', 'î', 'ï', 'Ù', 'Ú', 'Û', 'Ü', 'ù', 'ú', 'û', 'ü', 'ÿ', 'Ñ', 'ñ', 'Ç', 'ç', '°' };
            char[] newChar = { 'A', 'A', 'A', 'A', 'A', 'A', 'a', 'a', 'a', 'a', 'a', 'a', 'O', 'O', 'O', 'O', 'O', 'O', 'o', 'o', 'o', 'o', 'o', 'o', 'E', 'E', 'E', 'E', 'e', 'e', 'e', 'e', 'I', 'I', 'I', 'I', 'i', 'i', 'i', 'i', 'U', 'U', 'U', 'U', 'u', 'u', 'u', 'u', 'y', 'N', 'n', 'C', 'c', ' ' };
            for (var i = 0; i < oldChar.Length; i++)
            {
                libelle = libelle.Replace(oldChar[i], newChar[i]);
            }
            return libelle;
        }

        public static string RemoveBeginningWhiteSpace(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            var startIndex = 0;

            foreach (var c in str.ToCharArray())
            {
                if (c == ' ')
                {
                    startIndex++;
                }
                else
                {
                    break;
                }
            }
            return startIndex == 0 ? str : str.Substring(startIndex, str.Length - 1);
        }

        public static Color GetColorFromHexString(string hexValue)
        {
            var a = Convert.ToByte(hexValue.Substring(0, 2), 16);
            var r = Convert.ToByte(hexValue.Substring(2, 2), 16);
            var g = Convert.ToByte(hexValue.Substring(4, 2), 16);
            var b = Convert.ToByte(hexValue.Substring(6, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public static async Task ShowOrWaitAsync(this MessageDialog dialog)
        {
            await semaphore.WaitAsync();

            try
            {
                await dialog.ShowOrWaitAsync();
            }
            finally
            {
                semaphore.Release();
            }
        }


        internal static bool IsPaid(string p)
        {
            var licenseInformation = CurrentApp.LicenseInformation;
            return licenseInformation.ProductLicenses[p].IsActive;
        }

        internal static async Task<bool> TryBuy(string p)
        {
            var loader = new lang.Resources.ResourceLoader();
            var licenseInformation = CurrentApp.LicenseInformation;
            var error = false;

            if (!licenseInformation.ProductLicenses[p].IsActive)
            {
                try
                {
                    await CurrentApp.RequestProductPurchaseAsync(p);
                }
                catch (Exception)
                {
                    error = true;
                }
                if (error)
                {
                    await new MessageDialog(loader.GetString("BuyError")).ShowOrWaitAsync();
                    return false;
                }
                return true;
            }
            await new MessageDialog(loader.GetString("AlreadyBuy")).ShowOrWaitAsync();
            return false;
        }
        public static bool Between(this double num, double lower, double upper, bool inclusive = false)
        {
            return inclusive
                ? lower <= num && num <= upper
                : lower < num && num < upper;
        }


        internal static bool IsOnePaid()
        {
            return IsPaid("RemoveAds") || IsPaid("UnlockDetailedItems");
        }

        public static async Task SendEmail(string to, string content)
        {
           /* var em = new EmailMessage();
            em.To.Add(new EmailRecipient(to));
            em.Subject = "Sujet...";
            em.Body = "Email...\n\n\n\n\n --- Don't remove below there --- \n\n ETIC : " + Path.GetRandomFileName() + " Public : " + "{aoe4-rt7-1@a}\n\n---";
            await EmailManager.ShowComposeNewEmailAsync(em);*/

        }
    }
}
