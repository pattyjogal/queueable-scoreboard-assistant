using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace queueable_scoreboard_assistant.Common
{
    [Serializable]
    struct QueueRequest
    {
        public string JsonData { get; set; }
        public RequestAction Action { get; set; }

        public QueueRequest(string jsonData, RequestAction action)
        {
            this.JsonData = jsonData;
            this.Action = action;
        }

        private void WriteRequestString(DataWriter dataWriter)
        {
            string topLevelJson = JsonConvert.SerializeObject(this);
            dataWriter.WriteUInt32(dataWriter.MeasureString(topLevelJson));
            dataWriter.WriteString(topLevelJson);
        }

        public async void Send(DataWriter dataWriter)
        {
            WriteRequestString(dataWriter);
            await dataWriter.StoreAsync();
        }
    }

    enum RequestAction
    {
        HELLO,
        QUEUE_PROPAGATE
    }
}
