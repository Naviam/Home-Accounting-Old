using System;
using System.Web;

namespace Naviam.WebUI.Helpers.Cookies
{
	public interface ICookieContainer
	{
        void SetAuthCookie(string sessionKey, string userName, bool rememberMe);

		bool Exists(string key);
        
		string GetValue(string key);

		T GetValue<T>(string key);
		
		void SetValue(string key, object value, DateTime expires);
	}
}