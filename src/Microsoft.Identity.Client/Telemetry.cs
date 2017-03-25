﻿//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Identity.Client
{
    public class Telemetry
    {
        public delegate void Receiver(List<Dictionary<string, string>> events);

        private Receiver _receiver = null;

        public void RegisterReceiver(Receiver r)
        {
            _receiver = r;
        }

        private static readonly Telemetry Singleton = new Telemetry();

        private Telemetry(){}

        public static Telemetry GetInstance()
        {
            return Singleton;
        }

        internal EventCollection CreateEventCollection()  // An equivalent of CreateRequestID(...)
        {
            return new EventCollection(_receiver);
        }

        internal void Flush(EventCollection collection)
        {
            collection.Flush();
        }
    }
}