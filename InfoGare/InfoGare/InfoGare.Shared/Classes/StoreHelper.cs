using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Fasolib.Enums;

namespace InfoGare.Classes
{
    class StoreHelper
    {
        private static bool IsPaid(string p)
        {
            var licenseInformation = CurrentApp.LicenseInformation;
            return licenseInformation.ProductLicenses[p].IsActive;
        }

        internal static async Task<StoreResponse> TryBuy(string p)
        {
            var loader = new ResourceLoader();
            var licenseInformation = CurrentApp.LicenseInformation;

            if (licenseInformation.ProductLicenses[p].IsActive) return StoreResponse.AlreadyBought;

            try
            {
                await CurrentApp.RequestProductPurchaseAsync(p);
            }
            catch (Exception)
            {
                return StoreResponse.ServerError;
            }
            return StoreResponse.Bought;
        }

        public static bool IsPremium()
        {
            return IsPaid("RemoveAds") || IsPaid("UnlockDetailedItems");
        }
    }
}
