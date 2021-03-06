﻿//-----------------------------------------------------------------------
// <copyright file="HttpDeliveryMethods.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;

	/// <summary>
	/// The methods available for the local party to send messages to a remote party.
	/// </summary>
	/// <remarks>
	/// See OAuth 1.0 spec section 5.2.
	/// </remarks>
	[Flags]
	public enum HttpDeliveryMethods {
		/// <summary>
		/// No HTTP methods are allowed.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// In the HTTP Authorization header as defined in OAuth HTTP Authorization Scheme (OAuth HTTP Authorization Scheme).
		/// </summary>
		AuthorizationHeaderRequest = 0x1,

		/// <summary>
		/// As the HTTP POST request body with a content-type of application/x-www-form-urlencoded.
		/// </summary>
		PostRequest = 0x2,

		/// <summary>
		/// Added to the URLs in the query part (as defined by [RFC3986] (Berners-Lee, T., “Uniform Resource Identifiers (URI): Generic Syntax,” .) section 3).
		/// </summary>
		GetRequest = 0x4,
	}
}
