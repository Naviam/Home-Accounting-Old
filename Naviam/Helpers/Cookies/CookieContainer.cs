using System;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.Security;

namespace Naviam.WebUI.Helpers.Cookies
{
	public class CookieContainer : ICookieContainer
	{
		private readonly HttpRequestBase _request;
		private readonly HttpResponseBase _response;

		public CookieContainer()
		{
			var httpContext = new HttpContextWrapper(HttpContext.Current);
			_request = httpContext.Request;
			_response = httpContext.Response;
		}

		public CookieContainer(HttpRequestBase request, HttpResponseBase response)
		{
			//Check.IsNotNull(request, "request");
			//Check.IsNotNull(response, "response");

			_request = request;
			_response = response;
		}

		#region ICookieContainer Members

        public void SetAuthCookie(string sessionKey, string userName, bool rememberMe)
        {
            var exp = DateTime.Now.Add(FormsAuthentication.Timeout);
            var ticket = new FormsAuthenticationTicket(1, userName.ToLower(), DateTime.Now, exp, rememberMe, sessionKey);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            if (rememberMe)
                cookie.Expires = ticket.Expiration;
            SetValue(cookie.Name, cookie.Value, cookie.Expires);
        }

		public bool Exists(string key)
		{
			//Check.IsNotEmpty(key, "key");

			return _request.Cookies[key] != null;
		}

		public string GetValue(string key)
		{
			//Check.IsNotEmpty(key, "key");

			HttpCookie cookie = _request.Cookies[key];
			return cookie != null ? cookie.Value : null;
		}

		public T GetValue<T>(string key)
		{
			string val = GetValue(key);

			if (val == null)
				return default(T);

			Type type = typeof (T);
			bool isNullable = type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof (Nullable<>));
			if (isNullable)
				type = new NullableConverter(type).UnderlyingType;

			return (T) Convert.ChangeType(val, type, CultureInfo.InvariantCulture);
		}

		public void SetValue(string key, object value, DateTime expires)
		{
			//Check.IsNotEmpty(key, "key");

			string strValue = CheckAndConvertValue(value);

			HttpCookie cookie = new HttpCookie(key, strValue) {Expires = expires};
			_response.Cookies.Set(cookie);
		}

		#endregion

		private static string CheckAndConvertValue(object value)
		{
			if (value == null)
				return null;

			if (value is string)
				return value.ToString();

			// only allow value types and nullable<value types>

			Type type = value.GetType();
			bool isTypeAllowed = false;

			if (type.IsValueType)
				isTypeAllowed = true;
			else
			{
				bool isNullable = type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof (Nullable<>));
				if (isNullable)
				{
					NullableConverter converter = new NullableConverter(type);
					Type underlyingType = converter.UnderlyingType;
					if (underlyingType.IsValueType)
						isTypeAllowed = true;
				}
			}

			if (!isTypeAllowed)
				throw new NotSupportedException("Only value types and Nullable<ValueType> are allowed!");

			return (string) Convert.ChangeType(value, typeof (string), CultureInfo.InvariantCulture);
		}
	}
}