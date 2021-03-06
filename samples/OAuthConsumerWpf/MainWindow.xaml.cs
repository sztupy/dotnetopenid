﻿namespace DotNetOpenAuth.Samples.OAuthConsumerWpf {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.Text;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using DotNetOpenAuth;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.Samples.OAuthConsumerWpf.WcfSampleService;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private InMemoryTokenManager googleTokenManager = new InMemoryTokenManager();
		private DesktopConsumer google;
		private string googleAccessToken;
		private InMemoryTokenManager wcfTokenManager = new InMemoryTokenManager();
		private DesktopConsumer wcf;
		private string wcfAccessToken;

		public MainWindow() {
			this.InitializeComponent();

			this.InitializeGoogleConsumer();
			this.InitializeWcfConsumer();
		}

		private void InitializeGoogleConsumer() {
			this.googleTokenManager.ConsumerKey = ConfigurationManager.AppSettings["googleConsumerKey"];
			this.googleTokenManager.ConsumerSecret = ConfigurationManager.AppSettings["googleConsumerSecret"];

			string pfxFile = ConfigurationManager.AppSettings["googleConsumerCertificateFile"];
			if (string.IsNullOrEmpty(pfxFile)) {
				this.google = new DesktopConsumer(GoogleConsumer.ServiceDescription, this.googleTokenManager);
			} else {
				string pfxPassword = ConfigurationManager.AppSettings["googleConsumerCertificatePassword"];
				var signingCertificate = new X509Certificate2(pfxFile, pfxPassword);
				var service = GoogleConsumer.CreateRsaSha1ServiceDescription(signingCertificate);
				this.google = new DesktopConsumer(service, this.googleTokenManager);
			}
		}

		private void InitializeWcfConsumer() {
			this.wcfTokenManager.ConsumerKey = "sampleconsumer";
			this.wcfTokenManager.ConsumerSecret = "samplesecret";
			MessageReceivingEndpoint oauthEndpoint = new MessageReceivingEndpoint(
				new Uri("http://localhost:65169/OAuthServiceProvider/OAuth.ashx"),
				HttpDeliveryMethods.PostRequest);
			this.wcf = new DesktopConsumer(
				new ServiceProviderDescription {
					RequestTokenEndpoint = oauthEndpoint,
					UserAuthorizationEndpoint = oauthEndpoint,
					AccessTokenEndpoint = oauthEndpoint,
					TamperProtectionElements = new DotNetOpenAuth.Messaging.ITamperProtectionChannelBindingElement[] {
						new HmacSha1SigningBindingElement(),
					},
				},
				this.wcfTokenManager);
		}

		private void beginAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			if (string.IsNullOrEmpty(this.googleTokenManager.ConsumerKey)) {
				MessageBox.Show(this, "You must modify the App.config or OAuthConsumerWpf.exe.config file for this application to include your Google OAuth consumer key first.", "Configuration required", MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}

			Authorize auth = new Authorize(
				this.google,
				(DesktopConsumer consumer, out string requestToken) =>
				GoogleConsumer.RequestAuthorization(
					consumer,
					GoogleConsumer.Applications.Contacts | GoogleConsumer.Applications.Blogger,
					out requestToken));
			bool? result = auth.ShowDialog();
			if (result.HasValue && result.Value) {
				this.googleAccessToken = auth.AccessToken;
				postButton.IsEnabled = true;

				XDocument contactsDocument = GoogleConsumer.GetContacts(this.google, this.googleAccessToken);
				var contacts = from entry in contactsDocument.Root.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom"))
							   select new { Name = entry.Element(XName.Get("title", "http://www.w3.org/2005/Atom")).Value, Email = entry.Element(XName.Get("email", "http://schemas.google.com/g/2005")).Attribute("address").Value };
				contactsGrid.Children.Clear();
				foreach (var contact in contacts) {
					contactsGrid.RowDefinitions.Add(new RowDefinition());
					TextBlock name = new TextBlock { Text = contact.Name };
					TextBlock email = new TextBlock { Text = contact.Email };
					Grid.SetRow(name, contactsGrid.RowDefinitions.Count - 1);
					Grid.SetRow(email, contactsGrid.RowDefinitions.Count - 1);
					Grid.SetColumn(email, 1);
					contactsGrid.Children.Add(name);
					contactsGrid.Children.Add(email);
				}
			}
		}

		private void postButton_Click(object sender, RoutedEventArgs e) {
			XElement postBodyXml = XElement.Parse(postBodyBox.Text);
			GoogleConsumer.PostBlogEntry(this.google, this.googleAccessToken, blogUrlBox.Text, postTitleBox.Text, postBodyXml);
		}

		private void beginWcfAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			var requestArgs = new Dictionary<string, string>();
			requestArgs["scope"] = "http://tempuri.org/IDataApi/GetName|http://tempuri.org/IDataApi/GetAge|http://tempuri.org/IDataApi/GetFavoriteSites";
			Authorize auth = new Authorize(
				this.wcf,
				(DesktopConsumer consumer, out string requestToken) => consumer.RequestUserAuthorization(requestArgs, null, out requestToken));
			bool? result = auth.ShowDialog();
			if (result.HasValue && result.Value) {
				this.wcfAccessToken = auth.AccessToken;
				wcfName.Content = CallService(client => client.GetName());
				wcfAge.Content = CallService(client => client.GetAge());
				wcfFavoriteSites.Content = CallService(client => string.Join(", ", client.GetFavoriteSites()));
			}
		}

		private T CallService<T>(Func<DataApiClient, T> predicate) {
			DataApiClient client = new DataApiClient();
			var serviceEndpoint = new MessageReceivingEndpoint(client.Endpoint.Address.Uri, HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest);
			if (this.wcfAccessToken == null) {
				throw new InvalidOperationException("No access token!");
			}
			WebRequest httpRequest = this.wcf.PrepareAuthorizedRequest(serviceEndpoint, this.wcfAccessToken);

			HttpRequestMessageProperty httpDetails = new HttpRequestMessageProperty();
			httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers[HttpRequestHeader.Authorization];
			using (OperationContextScope scope = new OperationContextScope(client.InnerChannel)) {
				OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
				return predicate(client);
			}
		}
	}
}
