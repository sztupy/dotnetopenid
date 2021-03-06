﻿//-----------------------------------------------------------------------
// <copyright file="OutgoingWebResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OutgoingWebResponseTests {
		/// <summary>
		/// Verifies that setting the Body property correctly converts to a byte stream.
		/// </summary>
		[TestMethod]
		public void SetBodyToByteStream() {
			var response = new OutgoingWebResponse();
			string stringValue = "abc";
			response.Body = stringValue;
			Assert.AreEqual(stringValue.Length, response.ResponseStream.Length);

			// Verify that the actual bytes are correct.
			Encoding encoding = new UTF8Encoding(false); // avoid emitting a byte-order mark
			var expectedBuffer = encoding.GetBytes(stringValue);
			var actualBuffer = new byte[stringValue.Length];
			Assert.AreEqual(stringValue.Length, response.ResponseStream.Read(actualBuffer, 0, stringValue.Length));
			CollectionAssert.AreEqual(expectedBuffer, actualBuffer);

			// Verify that the header was set correctly.
			Assert.AreEqual(encoding.HeaderName, response.Headers[HttpResponseHeader.ContentEncoding]);
		}
	}
}
