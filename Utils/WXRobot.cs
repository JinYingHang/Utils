using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// WXRobot
    /// </summary>
    public class WXRobot
    {

        /// <summary>
        ///企业微信-智能机器人消息推送
        /// </summary>
        /// <param name="robotAddress">机器人地址，在对应的企业微信群复制</param>
        /// <param name="msg">要通知的消息</param>
        /// <param name="phoneArr">需要被@的人的手机号列表,所有人="@all"</param>
        /// <returns>接口调用结果 示例:{"errcode":0,"errmsg":"ok"}</returns>
        public static async Task<string> SendMsg(string robotAddress, string msg, params string[] phoneArr) {
            try {
                var requestObj = new { msgtype = "text", text = new { content = msg, mentioned_mobile_list = phoneArr } };
                return await PostAsyncJson(robotAddress, JsonConvert.SerializeObject(requestObj));
            }
            catch (System.Exception) {
                throw;
            }
        }

        private static async Task<string> PostAsyncJson(string url, string json) {
            try {
                HttpClient client = new HttpClient();
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (System.Exception) {
                throw;
            }

        }
    }
}