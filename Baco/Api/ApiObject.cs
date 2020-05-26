using System;
using System.ComponentModel;

namespace Baco.Api
{

    /// <summary>
    /// Actions for the API
    /// </summary>
    [Flags]
    public enum ApiFlag : uint
    {
        [Description("API error")]
        Error = 0,
        [Description("Checking hash")]
        CheckHash = 1,
        [Description("Getting ID")]
        GetId = 2 << 0,
        [Description("Creating user")]
        PostUser = 2 << 1,
        [Description("Checking nick availability")]
        CheckNickAvailability = 8,
        [Description("Checking mail availability")]
        CheckMailInUse = 16,
        [Description("Finding users")]
        FindUsers = 32,
        [Description("Getting RSS feeders")]
        GetRSSSubscriptions = 64,
        [Description("Updating profile picture")]
        UpdateProfilePicture = 128,
        [Description("Getting profile picture")]
        GetProfilePicture = 256,
        [Description("Getting friends")]
        GetFriends = 512,
        [Description("Getting groups")]
        GetGroups = 1024,
        [Description("Getting petitions")]
        GetPetitions = 2048,
        [Description("Accepting petition")]
        AcceptPetition = 4096,
        [Description("Sending petition")]
        SendPetition = 8192
    }

    /// <summary>
    /// To be sent to the API through the server
    /// </summary>
    [Serializable]
    public struct ApiObject
    {
        /// <summary>
        /// Action to make in API
        /// </summary>
        public ApiFlag apiFlag;
        /// <summary>
        /// Data to pass to API
        /// </summary>
        public object data;

        /// <summary>
        /// Creates the object to communicate with the API
        /// </summary>
        /// <param name="apiFlag">API action</param>
        /// <param name="data">Needed data to the action</param>
        public ApiObject(ApiFlag apiFlag, object data) : this()
        {
            this.apiFlag = apiFlag;
            this.data = data;
        }
    }
}
