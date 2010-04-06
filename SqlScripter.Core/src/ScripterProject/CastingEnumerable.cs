#region Header
// Copyright 2005-2008 Janusz Skonieczny
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// Last changes made by:
// $Id: CastingEnumerable.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using Qualent.Util;

namespace WooYek.Collections.Generic {
    public class CastingEnumerable<T> : IEnumerable<T> {
        private readonly IEnumerable backbone;

        public CastingEnumerable(IEnumerable backbone) {
            Guard.NotNull(backbone, "backbone");
            this.backbone = backbone;
        }

        public IEnumerator<T> GetEnumerator() {
            return new CastingEnumerator<T>(backbone.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return backbone.GetEnumerator();
        }
    }

    public class CastingEnumerator<T> : IEnumerator<T> {
        private readonly IEnumerator backbone;
        private object current;

        public CastingEnumerator(IEnumerator backbone) {
            Guard.NotNull(backbone, "backbone");
            this.backbone = backbone;
        }

        public bool MoveNext() {
            return backbone.MoveNext();
        }

        public void Reset() {
            backbone.Reset();
        }

        object IEnumerator.Current {
            get { return current; }
        }

        public T Current {
            get { return (T)backbone.Current; }
        }

        public void Dispose() {
            IDisposable d = backbone as IDisposable;
            if (d != null) {
                d.Dispose();
            }
        }
    }
}