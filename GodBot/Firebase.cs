using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodBot
{
    class Firebase
    {
        IFirebaseClient firebaseClient;
        

        public void initFirebase(string authSecret, string basePath) {
            IFirebaseConfig firebaseConfig;
            firebaseConfig = new FirebaseConfig
            {
                AuthSecret = authSecret,
                BasePath = basePath,
            };

            firebaseClient = new FireSharp.FirebaseClient(firebaseConfig);
        }

        public async Task<Coins> getSingleUser(string path, ulong userId)
        {
            FirebaseResponse response = await firebaseClient.GetTaskAsync(path + "/" + userId);
            var key = JsonConvert.DeserializeObject<Coins>(response.Body);
            return key;
        }

        public async Task<Dictionary<string, Coins>> getData(string path) {
            Dictionary<string, Coins> keyHolder = new Dictionary<string, Coins>();
            FirebaseResponse response = await firebaseClient.GetTaskAsync(path);
            keyHolder = JsonConvert.DeserializeObject<Dictionary<string, Coins>>(response.Body);
            //foreach (var b in keyHolder) {
            //    var z = b.Value;
            //    var q = z.userId;
            //}
            //var list = JsonConvert.DeserializeObject<Coins>(response.Body);
            return keyHolder;
        }

        public async Task updateData(string path, string key, Coins data)
        {
            FirebaseResponse response = await firebaseClient.UpdateTaskAsync(path + "/" + key, data);
        }

        public async Task setData(string path, string key, Coins data)
        {
            SetResponse response = await firebaseClient.SetTaskAsync(path + "/" + key, data);
        }

        public async Task pushDataAsync(string path, Coins data)
        {
            //PushResponse response = await firebaseClient.PushTaskAsync(path, data);
            DateTime date = DateTime.Now;
            //DateTime utcdate = DateTime.Now;
            PushResponse response = await firebaseClient.PushTaskAsync("test", date);
        }
    }
}
